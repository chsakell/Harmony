using Harmony.Api.IntegrationTests.Harness;
using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// The anonymous rule: one row per endpoint, sent with no Authorization header, new random IDs and <c>{}</c> as
    /// the body (FR-020). Each row is its own result named after the rule, method and route (FR-014).
    /// </summary>
    public class AnonymousMatrixTests(ApiTestHost host)
    {
        [Theory]
        [Trait("Requirement", "FR-020")]
        [Trait("Requirement", "FR-101")]
        [MemberData(nameof(MatrixRows.AnonymousRefused), MemberType = typeof(MatrixRows))]
        public Task AnonymousRequest_ProtectedEndpoint_Returns401(string method, string route) =>
            RunAsync(method, route, Mark.Refused);

        [Theory]
        [Trait("Requirement", "FR-021")]
        [Trait("Requirement", "FR-103")]
        [MemberData(nameof(MatrixRows.AnonymousPublic), MemberType = typeof(MatrixRows))]
        public Task AnonymousRequest_PublicEndpoint_IsNot401(string method, string route) =>
            RunAsync(method, route, Mark.NotApplicable);

        /// <summary>Today's gaps, pinned for R-07: each passes only while the endpoint lets an anonymous caller in.</summary>
        [Theory]
        [Trait("Requirement", "FR-020")]
        [Trait("Requirement", "FR-040")]
        [MemberData(nameof(MatrixRows.AnonymousKnownGaps), MemberType = typeof(MatrixRows))]
        public Task AnonymousRequest_KnownGap_IsLetIn_KnownIssue(string method, string route) =>
            RunAsync(method, route, Mark.KnownGap);

        private async Task RunAsync(string method, string route, Mark mark)
        {
            if (MatrixRows.IsPlaceholder(method))
            {
                MatrixFile.Load().Lines.ShouldNotContain(line => line.Anonymous == mark, "the placeholder row stands in only while no line has this mark");
                return;
            }

            var api = await host.GetAsync();
            var line = MatrixFile.Load().Lines.Single(line => line.Method == method && line.Route == route);
            line.Anonymous.ShouldBe(mark);
            var endpoint = api.Discovery().Actions.Single(action => action.Method == method && action.Route == route);

            using var request = RequestBuilder.Anonymous(endpoint);
            var response = await RowRunner.SendAsync(api, endpoint, request, token: null, TestContext.Current.CancellationToken);

            var verdict = RowVerdict.Judge(Rule.Anonymous, mark, response, MatrixRows.DisplayName(Rule.Anonymous, method, route));
            verdict.Passed.ShouldBeTrue(verdict.Message);
        }
    }
}
