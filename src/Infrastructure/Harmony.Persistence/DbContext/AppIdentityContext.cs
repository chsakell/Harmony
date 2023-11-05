using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Harmony.Persistence.Identity;

namespace Harmony.Persistence.DbContext
{
    /// <summary>
    /// The identity db context for harmony users
    /// </summary>
    public abstract class AppIdentityContext : IdentityDbContext<HarmonyUser, HarmonyRole, string, 
        IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, 
        HarmonyRoleClaim, IdentityUserToken<string>>
    {
        protected AppIdentityContext(DbContextOptions options) : base(options)
        {
        }
       
    }
}
