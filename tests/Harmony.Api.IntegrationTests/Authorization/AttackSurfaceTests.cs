using Harmony.Api.IntegrationTests.Harness;
using Harmony.Application.Notifications.Email;
using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// What production no longer serves (US4): the debug endpoint (FR-130) and, outside Development, the API explorer
    /// (FR-131). The test host runs as Testing, so it stands for production here.
    /// </summary>
    public class AttackSurfaceTests(ApiTestHost host)
    {
        /// <summary>The card ID that <c>POST api/Test</c> hard-coded in its notification.</summary>
        private static readonly Guid TestEndpointCard = Guid.Parse("8D7FAE06-D1E3-4418-FD89-08DBF00B9F44");

        [Fact]
        [Trait("Requirement", "FR-130")]
        public async Task TestEndpoint_Owner_Returns404AndPublishesNothing()
        {
            var api = await host.GetAsync();
            using var client = api.CreateClient(api.Tokens.For(api.Owner));

            using var response = await client.PostAsync("api/Test", content: null, TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(404);
            api.Publisher.Published.ShouldNotContain(message =>
                message.Kind == PublishedKind.Email && message.Payload is CardDueTimeUpdatedNotification
                && ((CardDueTimeUpdatedNotification)message.Payload).Id == TestEndpointCard);
        }

        [Theory]
        [Trait("Requirement", "FR-131")]
        [InlineData("/swagger/index.html", false)]
        [InlineData("/swagger/index.html", true)]
        [InlineData("/swagger/v1/swagger.json", false)]
        [InlineData("/swagger/v1/swagger.json", true)]
        public async Task ApiExplorer_NotDevelopment_IsNotServed(string path, bool loggedIn)
        {
            var api = await host.GetAsync();
            using var client = api.CreateClient(loggedIn ? api.Tokens.For(api.Owner) : null);

            using var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldNotBeInRange(200, 299);
            (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldNotContain("\"openapi\"");
        }
    }
}
