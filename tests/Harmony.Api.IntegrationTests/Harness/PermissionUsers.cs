using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Harmony.Persistence.Identity;
using Harmony.Shared.Constants.Permission;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Harmony.Api.IntegrationTests.Harness
{
    /// <summary>
    /// Users whose only role is a custom role (<c>matrix-&lt;suffix&gt;</c>) holding exactly the given permissions, for the
    /// admin rule (FR-110 to FR-112, FR-114). Each permission set gets one role and one user per host, created the first
    /// time a test asks, through the host's Identity stores.
    /// </summary>
    public static class PermissionUsers
    {
        private static readonly ConditionalWeakTable<ApiTestHost, ConcurrentDictionary<string, Lazy<Task<TestUser>>>> Users = new();

        public static Task<TestUser> UserWithPermissionsAsync(this ApiTestHost host, params string[] permissions)
        {
            var set = permissions.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
            var users = Users.GetValue(host, _ => new ConcurrentDictionary<string, Lazy<Task<TestUser>>>(StringComparer.Ordinal));
            return users.GetOrAdd(string.Join(';', set), _ => new Lazy<Task<TestUser>>(() => CreateAsync(host.Services, set))).Value;
        }

        private static async Task<TestUser> CreateAsync(IServiceProvider services, string[] permissions)
        {
            var roleName = "matrix-" + Guid.NewGuid().ToString("N")[..8];
            using (var scope = services.CreateScope())
            {
                var roles = scope.ServiceProvider.GetRequiredService<RoleManager<HarmonyRole>>();
                var role = new HarmonyRole(roleName, "Matrix test role: " + string.Join(", ", permissions));
                Succeeded(await roles.CreateAsync(role), "create the role", roleName);
                foreach (var permission in permissions)
                {
                    Succeeded(await roles.AddClaimAsync(role, new Claim(ApplicationClaimTypes.Permission, permission)), $"add {permission} to", roleName);
                }
            }

            return await TestUsers.CreateAsync(services, "perm", roleName);
        }

        private static void Succeeded(IdentityResult result, string action, string roleName)
        {
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not {action} {roleName}: {string.Join("; ", result.Errors.Select(error => error.Description))}");
            }
        }
    }
}
