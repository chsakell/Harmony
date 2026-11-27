using Harmony.Api.IntegrationTests.Harness;
using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// The routed endpoints outside the matrix that stay reachable without a login under the login-required default:
    /// the health checks (FR-104) and the error page that the exception handler re-runs (FR-106). The gRPC services are
    /// in <see cref="GrpcAnonymousTests"/>.
    /// </summary>
    public class ExplicitlyAnonymousTests(ApiTestHost host)
    {
        /// <summary>The text of <c>Pages/Error.cshtml</c>.</summary>
        private const string ErrorPageText = "An error occurred while processing your request.";

        [Theory]
        [Trait("Requirement", "FR-104")]
        [InlineData("/healthz/ready")]
        [InlineData("/healthz/live")]
        public async Task Health_Anonymous_Returns200(string path)
        {
            var api = await host.GetAsync();
            using var client = api.CreateClient();

            using var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(200);
        }

        /// <summary>
        /// A public request for a user that does not exist throws today (spec, FR-106 scenario). The test host runs as
        /// Testing, so the exception handler re-runs the request at /Error.
        /// </summary>
        [Fact]
        [Trait("Requirement", "FR-106")]
        public async Task ErrorPage_AnonymousRequestThatThrows_Returns500WithErrorPage()
        {
            var api = await host.GetAsync();
            using var client = api.CreateClient();

            using var response = await client.GetAsync($"api/identity/user/confirm-email?userId={Guid.NewGuid()}&code=x",
                TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(500);
            (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldContain(ErrorPageText);
        }
    }
}
