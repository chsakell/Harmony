using Harmony.Persistence.Identity;
using Harmony.Shared.Constants.Permission;
using Harmony.Shared.Constants.Role;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Harmony.Api.IntegrationTests.Harness
{
    public sealed record TestUser(string Id, string UserName, string Email, string FirstName, string LastName, IReadOnlyList<string> Roles)
    {
        /// <summary>The user's password, for a login through <c>POST api/identity/token</c>.</summary>
        public string Password { get; init; } = string.Empty;

        /// <summary>The permission claim values of the user's roles, as the login carries them (FR-140).</summary>
        public IReadOnlyList<string> Permissions { get; init; } = [];
    }

    /// <summary>
    /// Creates users directly through the host's Identity stores, not through the endpoints under test. Without
    /// <c>roles</c>, a user gets only the role a newly registered user gets (Basic; UserService.RegisterAsync).
    /// </summary>
    public static class TestUsers
    {
        public static async Task<TestUser> CreateAsync(IServiceProvider services, string label, params string[] roles)
        {
            if (roles.Length == 0)
            {
                roles = [RoleConstants.BasicRole];
            }

            using var scope = services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<HarmonyUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<HarmonyRole>>();

            var suffix = Guid.NewGuid().ToString("N")[..8];
            var password = Guid.NewGuid().ToString("N");
            var user = new HarmonyUser
            {
                UserName = $"{label}{suffix}",
                Email = $"{label}{suffix}@matrix.test",
                FirstName = label,
                LastName = suffix,
                IsActive = true,
                EmailConfirmed = true,
            };

            Succeeded(await users.CreateAsync(user, password), "create", user.UserName);
            Succeeded(await users.AddToRolesAsync(user, roles), $"add the roles {string.Join(", ", roles)} to", user.UserName);

            var permissions = new List<string>();
            foreach (var roleName in roles)
            {
                var role = await roleManager.FindByNameAsync(roleName)
                    ?? throw new InvalidOperationException($"The role {roleName} does not exist.");
                permissions.AddRange((await roleManager.GetClaimsAsync(role))
                    .Where(claim => claim.Type == ApplicationClaimTypes.Permission)
                    .Select(claim => claim.Value));
            }

            return new TestUser(user.Id, user.UserName, user.Email, user.FirstName, user.LastName, roles)
            {
                Password = password,
                Permissions = permissions.Distinct(StringComparer.Ordinal).ToList(),
            };
        }

        private static void Succeeded(IdentityResult result, string action, string userName)
        {
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not {action} test user {userName}: {string.Join("; ", result.Errors.Select(error => error.Description))}");
            }
        }
    }
}
