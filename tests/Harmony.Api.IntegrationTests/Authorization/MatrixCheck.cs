using System.Globalization;
using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// Compares the discovered endpoints with the matrix file: FR-011 (endpoint without a line), FR-012 (line without
    /// an endpoint), FR-013 (other endpoint outside the allowed set), FR-022 ([AllowAnonymous] on a non-public scope)
    /// and FR-042 (known-gap counts against the baselines). Pure code: it returns the problems, each naming its subject.
    /// </summary>
    public static class MatrixCheck
    {
        /// <summary>The gRPC services the Api serves outside the matrix (R-09; spec, <i>Other endpoints</i>).</summary>
        public static readonly IReadOnlySet<string> GrpcServices = new HashSet<string>(StringComparer.Ordinal)
        {
            "CardService", "UserCardService", "BoardService", "WorkspaceService", "UserService", "UserNotificationService",
        };

        private const string GrpcServiceNamespace = "Harmony.Api.Services.gRPC";

        private const string UnimplementedMethodSuffix = "/{unimplementedMethod:grpcunimplemented}";

        private static readonly string[] HealthRoutes = ["/healthz/ready", "/healthz/live"];

        public static IReadOnlyList<string> Problems(Discovery discovery, MatrixDocument document)
        {
            var problems = new List<string>();
            var lines = document.Lines.ToDictionary(line => (line.Route, line.Method));
            var endpoints = discovery.Actions.Select(action => (action.Route, action.Method)).ToHashSet();

            foreach (var action in discovery.Actions.OrderBy(action => action.Route, StringComparer.Ordinal).ThenBy(action => action.Method, StringComparer.Ordinal))
            {
                if (!lines.TryGetValue((action.Route, action.Method), out var line))
                {
                    problems.Add($"The endpoint {action.Name} ({action.Action.DisplayName}) has no line in the matrix file. State its scope: "
                        + $"add this line in sort order and set its scope (public, self, resource, administration or user) and marks: "
                        + $"| {action.Route} | {action.Method} | resource | refused | refused |  |");
                    continue;
                }

                if (action.AllowAnonymous && line.Scope != Scope.Public)
                {
                    problems.Add($"The endpoint {action.Name} allows anonymous access in code ([AllowAnonymous]) but its scope is "
                        + $"{line.Scope.ToString().ToLowerInvariant()} (line {line.LineNumber.ToString(CultureInfo.InvariantCulture)}). "
                        + "Remove [AllowAnonymous], or make it public in review (FR-022).");
                }
            }

            foreach (var line in document.Lines.Where(line => !endpoints.Contains((line.Route, line.Method))))
            {
                problems.Add($"Line {line.LineNumber.ToString(CultureInfo.InvariantCulture)} names no endpoint the host serves: {line.Text}");
            }

            foreach (var other in discovery.Others.Where(other => !IsAllowed(other)))
            {
                problems.Add($"The host serves an endpoint that is neither a controller action nor an allowed other endpoint (FR-013): "
                    + $"'{other.Name}' at '{other.Route ?? "(no route)"}'. Make it a controller action, or change the spec's allowed set.");
            }

            CheckBaseline(problems, "Anonymous", document.Count(Rule.Anonymous), document.Baselines.AnonymousGaps);
            CheckBaseline(problems, "Non-member", document.Count(Rule.NonMember), document.Baselines.NonMemberGaps);

            return problems;
        }

        private static void CheckBaseline(List<string> problems, string rule, int marks, int baseline)
        {
            if (marks != baseline)
            {
                problems.Add($"{rule} known gaps: the matrix file has {marks.ToString(CultureInfo.InvariantCulture)} mark{(marks == 1 ? string.Empty : "s")} "
                    + $"but the baseline {baseline.ToString(CultureInfo.InvariantCulture)} is recorded. Make the baseline equal the marks (FR-042).");
            }
        }

        private static bool IsAllowed(OtherEndpoint other)
        {
            var route = other.Route;
            var metadata = other.Endpoint.Metadata;

            var grpc = metadata.GetMetadata<GrpcMethodMetadata>();
            if (grpc is not null)
            {
                return grpc.ServiceType.Namespace == GrpcServiceNamespace && GrpcServices.Contains(grpc.ServiceType.Name);
            }

            if (route is null)
            {
                return false;
            }

            if (HealthRoutes.Contains(route, StringComparer.Ordinal))
            {
                return true;
            }

            if (route == "{*path:nonfile}" && other.Name.StartsWith("Fallback ", StringComparison.Ordinal))
            {
                return true;
            }

            if (metadata.GetMetadata<PageActionDescriptor>() is { ViewEnginePath: "/Error" })
            {
                return true;
            }

            if (route == "{unimplementedService}" + UnimplementedMethodSuffix)
            {
                return true;
            }

            return route.EndsWith(UnimplementedMethodSuffix, StringComparison.Ordinal)
                && GrpcServices.Contains(route[..^UnimplementedMethodSuffix.Length]);
        }
    }
}
