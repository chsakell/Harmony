using System.Net.Http.Json;
using Harmony.Api.IntegrationTests.Harness;
using Harmony.Application.Requests.Identity;
using Harmony.Application.Responses;
using Harmony.Persistence.Identity;
using Harmony.Shared.Constants.Permission;
using Harmony.Shared.Constants.Role;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// Administration beyond the admin rule's rows (US2): the 403 body (NFR-106), the extra permission a role rename
    /// needs (FR-114), and an administrator who logs in through the real token endpoint (FR-112).
    /// </summary>
    public class AdministrationTests(ApiTestHost host)
    {
        private const string Forbidden = "You are not authorized to access this resource.";

        [Fact]
        [Trait("Requirement", "NFR-106")]
        public async Task ForbiddenBody_PlainUser_IsResultEnvelope()
        {
            var api = await host.GetAsync();
            using var client = api.CreateClient(api.Tokens.For(api.NonMember));

            using var response = await client.GetAsync("api/identity/role", TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(403);
            (await LoginRequiredTests.MessagesAsync(response)).ShouldBe([Forbidden]);
        }

        /// <summary>A rename (a non-empty Id) needs Roles.Edit as well as the endpoint's Roles.Create.</summary>
        [Theory]
        [Trait("Requirement", "FR-114")]
        [Trait("Requirement", "NFR-106")]
        [InlineData(Permissions.Roles.Create)]
        [InlineData(Permissions.Roles.Edit)]
        public async Task RoleRename_OnlyCreateOrOnlyEdit_Returns403AndKeepsName(string permission)
        {
            var api = await host.GetAsync();
            var role = await CreateRoleAsync(api);
            using var client = api.CreateClient(api.Tokens.For(await api.UserWithPermissionsAsync(permission)));

            using var response = await client.PostAsJsonAsync("api/identity/role",
                new RoleRequest { Id = role.Id, Name = role.Name + "-renamed", Description = "renamed" }, TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(403);
            (await LoginRequiredTests.MessagesAsync(response)).ShouldBe([Forbidden]);
            (await RoleNameAsync(api, role.Id)).ShouldBe(role.Name);
        }

        [Fact]
        [Trait("Requirement", "FR-114")]
        public async Task RoleRename_CreateAndEdit_RenamesRole()
        {
            var api = await host.GetAsync();
            var role = await CreateRoleAsync(api);
            using var client = api.CreateClient(api.Tokens.For(await api.UserWithPermissionsAsync(Permissions.Roles.Create, Permissions.Roles.Edit)));

            using var response = await client.PostAsJsonAsync("api/identity/role",
                new RoleRequest { Id = role.Id, Name = role.Name + "-renamed", Description = "renamed" }, TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(200);
            (await RoleNameAsync(api, role.Id)).ShouldBe(role.Name + "-renamed");
        }

        [Fact]
        [Trait("Requirement", "FR-112")]
        public async Task Administrator_LoginThroughTokenEndpoint_ListsUsers()
        {
            var api = await host.GetAsync();
            var administrator = await TestUsers.CreateAsync(api.Services, "admin", RoleConstants.AdministratorRole);
            using var client = api.CreateClient(await TestTokenTests.LoginAsync(api, administrator));

            using var response = await client.GetAsync("api/identity/user", TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(200);
            var result = await response.Content.ReadFromJsonAsync<Result<List<UserResponse>>>(TestContext.Current.CancellationToken);
            result.ShouldNotBeNull().Succeeded.ShouldBeTrue(string.Join("; ", result.Messages));
            result.Data.ShouldContain(user => user.Id == administrator.Id);
        }

        private static async Task<HarmonyRole> CreateRoleAsync(ApiTestHost api)
        {
            var role = new HarmonyRole("rename-" + Guid.NewGuid().ToString("N")[..8], "Rename target");
            using var scope = api.Services.CreateScope();
            (await scope.ServiceProvider.GetRequiredService<RoleManager<HarmonyRole>>().CreateAsync(role)).Succeeded.ShouldBeTrue();
            return role;
        }

        private static async Task<string?> RoleNameAsync(ApiTestHost api, string roleId)
        {
            using var scope = api.Services.CreateScope();
            return (await scope.ServiceProvider.GetRequiredService<RoleManager<HarmonyRole>>().FindByIdAsync(roleId))?.Name;
        }
    }
}
