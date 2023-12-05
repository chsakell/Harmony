using Harmony.Application.Contracts.Persistence;
using Harmony.Infrastructure.Helpers;
using Harmony.Persistence.DbContext;
using Harmony.Persistence.Identity;
using Harmony.Shared.Constants.Permission;
using Harmony.Shared.Constants.Role;
using Harmony.Shared.Constants.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Harmony.Infrastructure.Seed
{
    /// <summary>
    /// Seeder for admin and basic user roles (optional)
    /// Adds two users
    /// </summary>
    public class DatabaseRolesSeeder : IDatabaseSeeder
    {
        public int Order => 1;

        private readonly ILogger<DatabaseRolesSeeder> _logger;
        private readonly IStringLocalizer<DatabaseRolesSeeder> _localizer;
        private readonly HarmonyContext _db;
        private readonly UserManager<HarmonyUser> _userManager;
        private readonly RoleManager<HarmonyRole> _roleManager;

        public DatabaseRolesSeeder(
            UserManager<HarmonyUser> userManager,
            RoleManager<HarmonyRole> roleManager,
            HarmonyContext db,
            ILogger<DatabaseRolesSeeder> logger,
            IStringLocalizer<DatabaseRolesSeeder> localizer)
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

            try
            {
                await AddAdministrator();
                await AddBasicUser();
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
        }

        private async Task AddAdministrator()
        {
            //Check if Role Exists
            var adminRole = new HarmonyRole(RoleConstants.AdministratorRole, _localizer["Administrator role with full permissions"]);
            var adminRoleInDb = await _roleManager.FindByNameAsync(RoleConstants.AdministratorRole);

            if (adminRoleInDb == null)
            {
                await _roleManager.CreateAsync(adminRole);
                adminRoleInDb = await _roleManager.FindByNameAsync(RoleConstants.AdministratorRole);
                _logger.LogInformation(_localizer["Seeded Administrator Role."]);
            }
            //Check if User Exists
            var superUser = new HarmonyUser
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "admin@harmony.com",
                UserName = "administrator",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                DateCreated = DateTime.Now,
                IsActive = true
            };

            var superUserInDb = await _userManager.FindByEmailAsync(superUser.Email);

            if (superUserInDb == null)
            {
                var userCreateResult = await _userManager.CreateAsync(superUser, UserConstants.DefaultPassword);
                var result = await _userManager.AddToRoleAsync(superUser, RoleConstants.AdministratorRole);
                if (result.Succeeded)
                {
                    _logger.LogInformation(_localizer["Seeded Default SuperAdmin User."]);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError(error.Description);
                    }
                }
            }

            foreach (var permission in Permissions.GetRegisteredPermissions())
            {
                await _roleManager.AddPermissionClaim(adminRoleInDb, permission);
            }
        }

        private async Task AddBasicUser()
        {
            //Check if Role Exists
            var basicRole = new HarmonyRole(RoleConstants.BasicRole, _localizer["Basic role with default permissions"]);
            var basicRoleInDb = await _roleManager.FindByNameAsync(RoleConstants.BasicRole);
            if (basicRoleInDb == null)
            {
                await _roleManager.CreateAsync(basicRole);
                _logger.LogInformation(_localizer["Seeded Basic Role."]);
            }
            //Check if User Exists
            var basicUser = new HarmonyUser
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@harmony.com",
                UserName = "johndoe",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                DateCreated = DateTime.Now,
                IsActive = true
            };

            var basicUserInDb = await _userManager.FindByEmailAsync(basicUser.Email);

            if (basicUserInDb == null)
            {
                await _userManager.CreateAsync(basicUser, UserConstants.DefaultPassword);
                await _userManager.AddToRoleAsync(basicUser, RoleConstants.BasicRole);
                _logger.LogInformation(_localizer["Seeded User with Basic Role."]);
            }
        }
    }
}
