using System.Security.Cryptography;
using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Harness
{
    /// <summary>
    /// The harness's tokens are accepted by the test host as the user they name, and only when they are signed with
    /// the run's key (FR-007). <c>GET api/Board/userboards</c> is one of the six endpoints with [Authorize] today.
    /// </summary>
    [Trait("Requirement", "FR-007")]
    public class TestTokenTests(ApiTestHost host)
    {
        private const string UserBoards = "api/Board/userboards";

        [Fact]
        public async Task UserBoards_HarnessToken_IsNot401()
        {
            var api = await host.GetAsync();
            using var client = api.CreateClient(api.Tokens.For(api.Owner));

            using var response = await client.GetAsync(UserBoards, TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldNotBe(401);
        }

        /// <summary>
        /// Today the Api answers 500 here (OnAuthenticationFailed writes to a started response); R-11 owns that status.
        /// </summary>
        [Fact]
        public async Task UserBoards_TokenSignedWithOtherKey_IsNot2xx()
        {
            var api = await host.GetAsync();
            var otherKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            using var client = api.CreateClient(TestTokens.ForKey(api.Owner, otherKey));

            using var response = await client.GetAsync(UserBoards, TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldNotBeInRange(200, 299);
        }
    }
}
