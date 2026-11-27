using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Harmony.Application.Requests.Identity;
using Harmony.Application.Responses;
using Harmony.Shared.Constants.Permission;
using Harmony.Shared.Constants.Role;
using Harmony.Shared.Wrapper;
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

        /// <summary>The harness token carries the permission claims that a real login carries (FR-140).</summary>
        [Fact]
        [Trait("Requirement", "FR-140")]
        public async Task Tokens_AdministratorUser_HarnessAndLoginCarrySamePermissionClaims()
        {
            var api = await host.GetAsync();
            var administrator = await TestUsers.CreateAsync(api.Services, "admin", RoleConstants.AdministratorRole);

            var login = await LoginAsync(api, administrator);

            var expected = PermissionClaims(login);
            expected.ShouldNotBeEmpty("the seeder gives the Administrator role every permission");
            PermissionClaims(api.Tokens.For(administrator)).ShouldBe(expected);
        }

        /// <summary>A user whose only role holds one permission gets exactly that permission, from the harness and the login.</summary>
        [Fact]
        [Trait("Requirement", "FR-140")]
        public async Task Tokens_PermissionUser_CarriesExactlyItsRolePermissions()
        {
            var api = await host.GetAsync();

            var user = await api.UserWithPermissionsAsync(Permissions.Roles.View);

            user.Roles.ShouldHaveSingleItem().ShouldStartWith("matrix-");
            PermissionClaims(api.Tokens.For(user)).ShouldBe([Permissions.Roles.View]);
            PermissionClaims(await LoginAsync(api, user)).ShouldBe([Permissions.Roles.View]);
            (await api.UserWithPermissionsAsync(Permissions.Roles.View)).ShouldBeSameAs(user, "users are created once per permission set");
        }

        /// <summary>
        /// Sends <paramref name="user"/>'s credentials to <c>POST api/identity/token</c> and returns the token it issues.
        /// </summary>
        public static async Task<string> LoginAsync(ApiTestHost api, TestUser user)
        {
            using var client = api.CreateClient();
            using var response = await client.PostAsJsonAsync("api/identity/token",
                new TokenRequest { Email = user.Email, Password = user.Password }, TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(200);
            var result = await response.Content.ReadFromJsonAsync<Result<TokenResponse>>(TestContext.Current.CancellationToken);
            result.ShouldNotBeNull().Succeeded.ShouldBeTrue(string.Join("; ", result.Messages));
            return result.Data.Token;
        }

        private static List<string> PermissionClaims(string token) =>
            new JwtSecurityTokenHandler().ReadJwtToken(token).Claims
                .Where(claim => claim.Type == ApplicationClaimTypes.Permission)
                .Select(claim => claim.Value)
                .Order(StringComparer.Ordinal)
                .ToList();
    }
}
