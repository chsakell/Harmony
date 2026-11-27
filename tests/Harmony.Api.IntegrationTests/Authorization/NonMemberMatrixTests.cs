using Harmony.Api.IntegrationTests.Harness;
using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// The non-member rule: one row per resource endpoint. The non-member sends the request with a new fixture's IDs
    /// (FR-030, FR-031), and the owner sends the same request on another new fixture, which must not be refused for the
    /// row to count (FR-032, FR-033).
    /// </summary>
    public class NonMemberMatrixTests(ApiTestHost host)
    {
        [Theory]
        [Trait("Requirement", "FR-030")]
        [Trait("Requirement", "FR-033")]
        [MemberData(nameof(MatrixRows.NonMemberRefused), MemberType = typeof(MatrixRows))]
        public Task NonMemberRequest_ResourceEndpoint_Returns403Or404(string method, string route) =>
            RunAsync(method, route, Mark.Refused);

        /// <summary>Today's gaps, pinned for R-08: each passes only while the endpoint lets the non-member in.</summary>
        [Theory]
        [Trait("Requirement", "FR-030")]
        [Trait("Requirement", "FR-033")]
        [Trait("Requirement", "FR-040")]
        [MemberData(nameof(MatrixRows.NonMemberKnownGaps), MemberType = typeof(MatrixRows))]
        public Task NonMemberRequest_KnownGap_IsLetIn_KnownIssue(string method, string route) =>
            RunAsync(method, route, Mark.KnownGap);

        private async Task RunAsync(string method, string route, Mark mark)
        {
            if (MatrixRows.IsPlaceholder(method))
            {
                MatrixFile.Load().Lines.ShouldNotContain(line => line.Scope == Scope.Resource && line.NonMember == mark,
                    "the placeholder row stands in only while no resource line has this mark");
                return;
            }

            var api = await host.GetAsync();
            var line = MatrixFile.Load().Lines.Single(line => line.Method == method && line.Route == route);
            line.NonMember.ShouldBe(mark);
            var endpoint = api.Discovery().Actions.Single(action => action.Method == method && action.Route == route);
            var nonMemberFixture = await FixtureFactory.CreateAsync(api.Services, api.Owner);
            var ownerFixture = await FixtureFactory.CreateAsync(api.Services, api.Owner);

            var run = await RowRunner.RunNonMemberAsync(api, endpoint, line, nonMemberFixture, ownerFixture, TestContext.Current.CancellationToken);

            var verdict = RowVerdict.Judge(Rule.NonMember, mark, run.NonMember, MatrixRows.DisplayName(Rule.NonMember, method, route), run.Owner);
            verdict.Passed.ShouldBeTrue(verdict.Message);
        }
    }
}
