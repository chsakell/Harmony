using Harmony.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Persistence.Identity
{
    public class HarmonyRole : IdentityRole, IAuditableEntity<string>
    {
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        public ICollection<HarmonyRoleClaim> RoleClaims { get; set; }

        public HarmonyRole() : base()
        {
            RoleClaims = new HashSet<HarmonyRoleClaim>();
        }

        public HarmonyRole(string roleName, string roleDescription = null) : base(roleName)
        {
            RoleClaims = new HashSet<HarmonyRoleClaim>();
            Description = roleDescription;
        }
    }
}
