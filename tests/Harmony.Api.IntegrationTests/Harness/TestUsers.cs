using Harmony.Persistence.Identity;
using Harmony.Shared.Constants.Role;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Harmony.Api.IntegrationTests.Harness
{
    public sealed record TestUser(string Id, string UserName, string Email, string FirstName, string LastName, IReadOnlyList<string> Roles);

    /// <summary>
    /// Creates users directly through the host's Identity stores, not through the endpoints under test. Each gets
    /// only the role a newly registered user gets (Basic; UserService.RegisterAsync).
    /// </summary>
    public static class TestUsers
    {
        public static async Task<TestUser> CreateAsync(IServiceProvider services, string label)
        {
            using var scope = services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<HarmonyUser>>();

            var suffix = Guid.NewGuid().ToString("N")[..8];
            var user = new HarmonyUser
            {
                UserName = $"{label}{suffix}",
                Email = $"{label}{suffix}@matrix.test",
                FirstName = label,
                LastName = suffix,
                IsActive = true,
                EmailConfirmed = true,
            };

            Succeeded(await users.CreateAsync(user, Guid.NewGuid().ToString("N")), "create", user.UserName);
            Succeeded(await users.AddToRoleAsync(user, RoleConstants.BasicRole), "add the Basic role to", user.UserName);

            return new TestUser(user.Id, user.UserName, user.Email, user.FirstName, user.LastName, [RoleConstants.BasicRole]);
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
