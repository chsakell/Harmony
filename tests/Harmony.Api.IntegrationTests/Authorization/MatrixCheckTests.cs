using System.Reflection;
using Grpc.AspNetCore.Server;
using Grpc.Core;
using Harmony.Api.IntegrationTests.Harness;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// The matrix discovers its endpoints from the host's routing and fails when the matrix file and the host disagree.
    /// The <c>Check_*</c> tests run on synthetic endpoints; two tests check the real host and file.
    /// </summary>
    public class MatrixCheckTests(ApiTestHost host)
    {
        [Fact]
        [Trait("Requirement", "FR-010")]
        public async Task Discovery_TestHost_FindsOneEndpointPerControllerAction()
        {
            var api = await host.GetAsync();

            var discovery = EndpointDiscovery.Discover(api.Services);

            var actionMethods = typeof(Program).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && typeof(ControllerBase).IsAssignableFrom(type))
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .Where(method => method.GetCustomAttributes<HttpMethodAttribute>().Any())
                .ToList();
            var discoveredMethods = discovery.Actions.Select(action => ((ControllerActionDescriptor)action.Action).MethodInfo).ToList();

            discoveredMethods.ShouldBeUnique();
            discoveredMethods.ShouldBe(actionMethods, ignoreOrder: true);
            discovery.Actions.Count.ShouldBe(105, "the count on 2026-11-27, after #25 removed POST api/Test (spec #22, FR-010); update it with the matrix file");
            discovery.Actions.ShouldAllBe(action => action.Api != null);
        }

        [Fact]
        [Trait("Requirement", "FR-010")]
        [Trait("Requirement", "FR-011")]
        [Trait("Requirement", "FR-012")]
        [Trait("Requirement", "FR-013")]
        [Trait("Requirement", "FR-022")]
        [Trait("Requirement", "FR-042")]
        [Trait("Requirement", "NFR-101")]
        public async Task TestHostAndMatrixFile_AreConsistent()
        {
            var api = await host.GetAsync();

            var problems = MatrixCheck.Problems(EndpointDiscovery.Discover(api.Services), MatrixFile.Load());

            problems.ShouldBeEmpty(string.Join(Environment.NewLine, problems));
        }

        [Fact]
        [Trait("Requirement", "FR-011")]
        public void Check_EndpointWithoutLine_FailsNamingItAndAskingForScope()
        {
            var discovery = Discover(Action("GET", "api/Alpha"), Action("POST", "api/Beta/{id:guid}"));

            var problems = MatrixCheck.Problems(discovery, Document("| api/Alpha | GET | resource | gap R-07 | refused | - |  |"));

            var problem = problems.ShouldHaveSingleItem();
            problem.ShouldContain("POST api/Beta/{id:guid}");
            problem.ShouldContain("scope");
            problem.ShouldContain("| api/Beta/{id:guid} | POST |");
        }

        [Fact]
        [Trait("Requirement", "FR-012")]
        public void Check_LineWithoutEndpoint_FailsNamingLine()
        {
            const string stale = "| api/Gone | DELETE | resource | gap R-07 | refused | - |  |";
            var discovery = Discover(Action("GET", "api/Alpha"));

            var problems = MatrixCheck.Problems(discovery, Document("| api/Alpha | GET | resource | gap R-07 | refused | - |  |", stale));

            var problem = problems.ShouldHaveSingleItem();
            problem.ShouldContain("no endpoint");
            problem.ShouldContain(stale);
        }

        [Fact]
        [Trait("Requirement", "FR-013")]
        public void Check_AllowedOtherEndpoints_Pass()
        {
            var discovery = Discover(
                Action("GET", "api/Alpha"),
                Other("/healthz/ready", "Health checks"),
                Other("/healthz/live", "Health checks"),
                Other("{*path:nonfile}", "Fallback {*path:nonfile}", new HttpMethodMetadata(["GET", "HEAD"])),
                Other("Error", "/Error", new Microsoft.AspNetCore.Mvc.RazorPages.PageActionDescriptor { ViewEnginePath = "/Error" }),
                Other("/CardService/GetCard", "gRPC - /CardService/GetCard", Grpc(typeof(Harmony.Api.Services.gRPC.CardService))),
                Other("{unimplementedService}/{unimplementedMethod:grpcunimplemented}", "gRPC - Unimplemented service"),
                Other("CardService/{unimplementedMethod:grpcunimplemented}", "gRPC - Unimplemented method for CardService"));

            MatrixCheck.Problems(discovery, Document("| api/Alpha | GET | resource | gap R-07 | refused | - |  |")).ShouldBeEmpty();
        }

        [Fact]
        [Trait("Requirement", "FR-013")]
        public void Check_UnexpectedOtherEndpoint_FailsNamingIt()
        {
            var discovery = Discover(Action("GET", "api/Alpha"), Other("/minimal", "HTTP: GET /minimal", new HttpMethodMetadata(["GET"])));

            var problems = MatrixCheck.Problems(discovery, Document("| api/Alpha | GET | resource | gap R-07 | refused | - |  |"));

            var problem = problems.ShouldHaveSingleItem();
            problem.ShouldContain("/minimal");
            problem.ShouldContain("HTTP: GET /minimal");
        }

        [Theory]
        [Trait("Requirement", "FR-013")]
        [InlineData("/AuditService/Read", "gRPC - /AuditService/Read", true)]
        [InlineData("AuditService/{unimplementedMethod:grpcunimplemented}", "gRPC - Unimplemented method for AuditService", false)]
        public void Check_UnknownGrpcService_Fails(string route, string displayName, bool withMetadata)
        {
            var other = withMetadata ? Other(route, displayName, Grpc(typeof(AuditService))) : Other(route, displayName);
            var discovery = Discover(Action("GET", "api/Alpha"), other);

            var problems = MatrixCheck.Problems(discovery, Document("| api/Alpha | GET | resource | gap R-07 | refused | - |  |"));

            problems.ShouldHaveSingleItem().ShouldContain("AuditService");
        }

        [Fact]
        [Trait("Requirement", "FR-022")]
        public void Check_AllowAnonymousOnNonPublicScope_FailsNamingIt()
        {
            var discovery = Discover(
                Action("GET", "api/identity/user", new AllowAnonymousAttribute()),
                Action("POST", "api/identity/user", new AllowAnonymousAttribute()));

            var problems = MatrixCheck.Problems(discovery, Document(
                "| api/identity/user | GET | administration | refused | - | refused | permission=Permissions.Users.View |",
                "| api/identity/user | POST | public | - | - | - |  |"));

            var problem = problems.ShouldHaveSingleItem();
            problem.ShouldContain("GET api/identity/user");
            problem.ShouldContain("[AllowAnonymous]");
            problem.ShouldContain("administration");
        }

        [Fact]
        [Trait("Requirement", "FR-107")]
        public void Check_PublicLineOutsideTheSix_FailsNamingLine()
        {
            const string seventh = "| api/identity/account | PUT | public | - | - | - |  |";
            var discovery = Discover(Action("POST", "api/identity/token"), Action("PUT", "api/identity/account"));

            var problems = MatrixCheck.Problems(discovery, Document(seventh, "| api/identity/token | POST | public | - | - | - |  |"));

            var problem = problems.ShouldHaveSingleItem();
            problem.ShouldContain(seventh);
            problem.ShouldContain("not one of the six public endpoints");
        }

        [Fact]
        [Trait("Requirement", "FR-107")]
        public void Check_OneOfTheSixNotPublic_FailsNamingLine()
        {
            const string token = "| api/identity/token | POST | self | refused | - | - |  |";
            var discovery = Discover(Action("POST", "api/identity/token"));

            var problems = MatrixCheck.Problems(discovery, Document(token));

            var problem = problems.ShouldHaveSingleItem();
            problem.ShouldContain(token);
            problem.ShouldContain("must be public");
        }

        [Fact]
        [Trait("Requirement", "FR-042")]
        public void Check_BaselineDiffers_FailsGivingBothNumbers()
        {
            var discovery = Discover(Action("GET", "api/Alpha"), Action("GET", "api/Beta"));
            var document = Document(anonymousGaps: 1, nonMemberGaps: 2, "| api/Alpha | GET | resource | refused | gap R-08 | - |  |", "| api/Beta | GET | resource | refused | refused | - |  |");

            var problems = MatrixCheck.Problems(discovery, document);

            problems.Count.ShouldBe(2);
            problems.ShouldContain(problem => problem.Contains("Anonymous") && problem.Contains("0 marks") && problem.Contains("baseline 1"));
            problems.ShouldContain(problem => problem.Contains("Non-member") && problem.Contains("1 mark") && problem.Contains("baseline 2"));
        }

        [Fact]
        [Trait("Requirement", "FR-010")]
        public void Discover_ActionWithoutOneHttpMethod_Throws()
        {
            var endpoint = new RouteEndpoint(_ => Task.CompletedTask, RoutePatternFactory.Parse("api/Any"), 0,
                new EndpointMetadataCollection(new ControllerActionDescriptor { DisplayName = "AnyController.Any" }), "AnyController.Any");

            Should.Throw<InvalidOperationException>(() => EndpointDiscovery.Discover([endpoint], []))
                .Message.ShouldContain("AnyController.Any");
        }

        private static Discovery Discover(params Endpoint[] endpoints) => EndpointDiscovery.Discover(endpoints, []);

        private static RouteEndpoint Action(string method, string route, params object[] metadata) =>
            new(_ => Task.CompletedTask, RoutePatternFactory.Parse(route), 0,
                new EndpointMetadataCollection([new HttpMethodMetadata([method]), new ControllerActionDescriptor { DisplayName = $"{method} {route}" }, .. metadata]),
                $"{method} {route}");

        private static RouteEndpoint Other(string route, string displayName, params object[] metadata) =>
            new(_ => Task.CompletedTask, RoutePatternFactory.Parse(route), 0, new EndpointMetadataCollection(metadata), displayName);

        private static GrpcMethodMetadata Grpc(Type serviceType) =>
            new(serviceType, new Method<string, string>(MethodType.Unary, serviceType.Name, "Read", Marshallers.StringMarshaller, Marshallers.StringMarshaller));

        private static MatrixDocument Document(params string[] lines) =>
            Document(lines.Count(line => line.Contains("gap R-07", StringComparison.Ordinal)), lines.Count(line => line.Contains("gap R-08", StringComparison.Ordinal)), lines);

        private static MatrixDocument Document(int anonymousGaps, int nonMemberGaps, params string[] lines) =>
            MatrixFile.Parse(string.Join('\n',
            [
                "## Baselines",
                $"- Anonymous known gaps: {anonymousGaps}",
                $"- Non-member known gaps: {nonMemberGaps}",
                "## Endpoints",
                MatrixFile.Header,
                "|---|---|---|---|---|---|---|",
                .. lines,
            ]));

        /// <summary>A gRPC service the host must not serve unnoticed.</summary>
        private sealed class AuditService;
    }
}
