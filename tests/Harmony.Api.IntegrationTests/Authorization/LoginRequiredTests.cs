using System.Net.Http.Json;
using System.Text.Json;
using Harmony.Api.IntegrationTests.Harness;
using Harmony.Application.Requests.Identity;
using Harmony.Application.Responses;
using Harmony.Persistence.Identity;
using Harmony.Shared.Constants.Role;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// The login-required default beyond the matrix rows (US1): the fallback policy itself (FR-102), the takeover path
    /// on a real role and user (FR-101), the public login (FR-103), an expired login (FR-108), routes that match no
    /// endpoint (FR-109) and the 401 bodies (NFR-106).
    /// </summary>
    public class LoginRequiredTests(ApiTestHost host)
    {
        private const string NotAuthorized = "You are not Authorized.";

        private const string TokenExpired = "The Token is expired.";

        private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

        /// <summary>Endpoints without an attribute are covered because the fallback policy needs a login.</summary>
        [Fact]
        [Trait("Requirement", "FR-102")]
        public async Task FallbackPolicy_TestHost_RequiresAuthenticatedUser()
        {
            var api = await host.GetAsync();

            var policy = await api.Services.GetRequiredService<IAuthorizationPolicyProvider>().GetFallbackPolicyAsync();

            policy.ShouldNotBeNull("the Api has no fallback policy, so an endpoint without an attribute is open (FR-102)")
                .Requirements.ShouldContain(requirement => requirement is DenyAnonymousAuthorizationRequirement);
        }

        [Fact]
        [Trait("Requirement", "FR-101")]
        public async Task AnonymousDeleteRole_ExistingRole_Returns401AndRoleRemains()
        {
            var api = await host.GetAsync();
            var role = new HarmonyRole("anon-" + Guid.NewGuid().ToString("N")[..8], "Anonymous delete target");
            using (var scope = api.Services.CreateScope())
            {
                (await scope.ServiceProvider.GetRequiredService<RoleManager<HarmonyRole>>().CreateAsync(role)).Succeeded.ShouldBeTrue();
            }

            using var client = api.CreateClient();
            using var response = await client.DeleteAsync($"api/identity/role/{role.Id}", TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(401);
            using (var scope = api.Services.CreateScope())
            {
                (await scope.ServiceProvider.GetRequiredService<RoleManager<HarmonyRole>>().FindByIdAsync(role.Id)).ShouldNotBeNull();
            }
        }

        [Fact]
        [Trait("Requirement", "FR-101")]
        public async Task AnonymousUpdateUserRoles_ExistingUser_Returns401AndRolesUnchanged()
        {
            var api = await host.GetAsync();
            var user = await TestUsers.CreateAsync(api.Services, "anonroles");

            using var client = api.CreateClient();
            using var response = await client.PutAsJsonAsync($"api/identity/user/roles/{user.Id}", new UpdateUserRolesRequest
            {
                UserId = user.Id,
                UserRoles = [new UserRoleModel { RoleName = RoleConstants.AdministratorRole, Selected = true }],
            }, TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(401);
            using var scope = api.Services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<HarmonyUser>>();
            (await users.GetRolesAsync((await users.FindByIdAsync(user.Id)).ShouldNotBeNull())).ShouldBe([RoleConstants.BasicRole]);
        }

        [Fact]
        [Trait("Requirement", "NFR-106")]
        public async Task AnonymousRequest_ProtectedEndpoint_ReturnsNotAuthorizedBody()
        {
            var api = await host.GetAsync();
            using var client = api.CreateClient();

            using var response = await client.GetAsync("api/Workspace", TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(401);
            (await MessagesAsync(response)).ShouldBe([NotAuthorized]);
        }

        /// <summary><c>api/Board/userboards</c> has [Authorize] today; <c>api/Workspace</c> has no attribute.</summary>
        [Theory]
        [Trait("Requirement", "FR-108")]
        [Trait("Requirement", "NFR-106")]
        [InlineData("api/Board/userboards")]
        [InlineData("api/Workspace")]
        public async Task ExpiredLogin_ProtectedEndpoint_Returns401WithExpiredBody(string route)
        {
            var api = await host.GetAsync();
            using var client = api.CreateClient(api.Tokens.For(api.Owner, expires: DateTime.UtcNow.AddMinutes(-1)));

            using var response = await client.GetAsync(route, TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(401);
            (await MessagesAsync(response)).ShouldBe([TokenExpired]);
        }

        [Theory]
        [Trait("Requirement", "FR-109")]
        [InlineData("GET")]
        [InlineData("POST")]
        public async Task UnknownRoute_Anonymous_Returns401(string method)
        {
            var api = await host.GetAsync();
            using var client = api.CreateClient();

            using var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), "api/nothing-here"),
                TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(401);
        }

        [Theory]
        [Trait("Requirement", "FR-109")]
        [InlineData("GET")]
        [InlineData("POST")]
        public async Task UnknownRoute_Owner_Returns404(string method)
        {
            var api = await host.GetAsync();
            using var client = api.CreateClient(api.Tokens.For(api.Owner));

            using var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), "api/nothing-here"),
                TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(404);
        }

        [Fact]
        [Trait("Requirement", "FR-103")]
        public async Task Token_HarnessUserCredentials_ReturnsToken()
        {
            var api = await host.GetAsync();
            var user = await TestUsers.CreateAsync(api.Services, "login");

            var token = await TestTokenTests.LoginAsync(api, user);

            token.ShouldNotBeNullOrWhiteSpace();
        }

        /// <summary>The result envelope's messages; the body must be exactly one envelope (NFR-106).</summary>
        internal static async Task<List<string>> MessagesAsync(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Result? result = null;
            Should.NotThrow(() => result = JsonSerializer.Deserialize<Result>(body, Web), $"the body is not one result envelope: {body}");
            result.ShouldNotBeNull($"body: {body}").Succeeded.ShouldBeFalse();
            return result.Messages;
        }
    }
}
