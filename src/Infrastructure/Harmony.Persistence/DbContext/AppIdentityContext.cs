using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony.Persistence.Identity;
using Harmony.Domain.Entities;

namespace Harmony.Persistence.DbContext
{
    public abstract class AppIdentityContext : IdentityDbContext<HarmonyUser, HarmonyRole, string, 
        IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, 
        HarmonyRoleClaim, IdentityUserToken<string>>
    {
        protected AppIdentityContext(DbContextOptions options) : base(options)
        {
        }
       
    }
}
