using Bogus;
using Harmony.Application.Contracts.Persistence;
using Harmony.Infrastructure.Helpers;
using Harmony.Persistence.DbContext;
using Harmony.Persistence.Identity;
using Harmony.Shared.Constants.Permission;
using Harmony.Shared.Constants.Role;
using Harmony.Shared.Constants.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using static Harmony.Shared.Storage.StorageConstants;

namespace Harmony.Infrastructure.Seed
{
    public class DatabaseUsersSeeder : IDatabaseSeeder
    {
        public int Order => 2;

        private readonly ILogger<DatabaseUsersSeeder> _logger;
        private readonly IStringLocalizer<DatabaseUsersSeeder> _localizer;
        private readonly HarmonyContext _db;
        private readonly UserManager<HarmonyUser> _userManager;
        private readonly RoleManager<HarmonyRole> _roleManager;

        public DatabaseUsersSeeder(
            UserManager<HarmonyUser> userManager,
            RoleManager<HarmonyRole> roleManager,
            HarmonyContext db,
            ILogger<DatabaseUsersSeeder> logger,
            IStringLocalizer<DatabaseUsersSeeder> localizer)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task Initialize()
        {
            _db.Database.EnsureCreated();

            await AddBasicUsers();
            await _db.SaveChangesAsync();
        }

        private async Task AddBasicUsers()
        {
            //Check if Role Exists
            var basicRole = new HarmonyRole(RoleConstants.BasicRole, _localizer["Basic role with default permissions"]);
            var basicRoleInDb = await _roleManager.FindByNameAsync(RoleConstants.BasicRole);
            if (basicRoleInDb == null)
            {
                await _roleManager.CreateAsync(basicRole);
                _logger.LogInformation(_localizer["Seeded Basic Role."]);
            }

            var totalUsers = await _userManager.Users.CountAsync();

            if(totalUsers > 5)
            {
                return;
            }

            for (int i = 0; i < 100; i++)
            {
                var hasher = new PasswordHasher<HarmonyUser>();
                var faker = new Faker("en");
                var userName = faker.Person.UserName;
                var user = new HarmonyUser()
                {
                    FirstName = faker.Person.FirstName,
                    LastName = faker.Person.LastName,
                    UserName = userName,
                    NormalizedUserName = userName.ToUpper(),
                    Email = faker.Person.Email,
                    EmailConfirmed = true,
                    PasswordHash = hasher.HashPassword(null, "P@ssw0rd1"),
                    IsActive = true
                };

                await _userManager.CreateAsync(user);
                await _userManager.AddToRoleAsync(user, RoleConstants.BasicRole);
            }
        }
    }
}
