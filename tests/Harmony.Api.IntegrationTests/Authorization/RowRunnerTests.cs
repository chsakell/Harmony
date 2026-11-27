using Harmony.Api.IntegrationTests.Harness;
using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// The runner tells whether a request reached the endpoint under test, through the request recorder (FR-015).
    /// </summary>
    public class RowRunnerTests(ApiTestHost host)
    {
        private const string Method = "POST";
        private const string Route = "api/Board/{id:guid}/load";

        [Fact]
        [Trait("Requirement", "FR-015")]
        public async Task Request_RouteValueBreaksGuidConstraint_IsInconclusive()
        {
            var api = await host.GetAsync();
            var endpoint = api.Discovery().Actions.Single(action => action.Method == Method && action.Route == Route);
            using var request = RequestBuilder.Anonymous(endpoint);
            request.RequestUri = new Uri("api/Board/not-a-guid/load", UriKind.Relative);

            var response = await RowRunner.SendAsync(api, endpoint, request, token: null, TestContext.Current.CancellationToken);

            response.ReachedEndpoint.ShouldBeFalse();
            RowVerdict.Classify(Rule.Anonymous, response).ShouldBe(Outcome.Inconclusive);
        }

        [Fact]
        [Trait("Requirement", "FR-015")]
        public async Task Request_WellFormed_ReachesEndpoint()
        {
            var api = await host.GetAsync();
            var endpoint = api.Discovery().Actions.Single(action => action.Method == Method && action.Route == Route);
            using var request = RequestBuilder.Anonymous(endpoint);

            var response = await RowRunner.SendAsync(api, endpoint, request, token: null, TestContext.Current.CancellationToken);

            response.ReachedEndpoint.ShouldBeTrue(response.MatchedEndpoint);
            response.MatchedEndpoint.ShouldBe(endpoint.Endpoint.DisplayName);
        }

        [Fact]
        [Trait("Requirement", "FR-020")]
        public async Task AnonymousRequest_FillsRouteIdsWithNewGuidsAndSendsEmptyBody()
        {
            var api = await host.GetAsync();
            var endpoint = api.Discovery().Actions.Single(action => action.Method == Method && action.Route == Route);

            using var first = RequestBuilder.Anonymous(endpoint);
            using var second = RequestBuilder.Anonymous(endpoint);

            first.Headers.Authorization.ShouldBeNull();
            var segments = first.RequestUri!.OriginalString.Split('/');
            segments.Length.ShouldBe(4);
            Guid.TryParse(segments[2], out _).ShouldBeTrue(first.RequestUri.OriginalString);
            second.RequestUri!.OriginalString.ShouldNotBe(first.RequestUri.OriginalString);
            (await first.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldBe("{}");
        }
    }
}
