using Harmony.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Harmony.Persistence.Identity
{
    /// <summary>
    /// Custom role claim
    /// </summary>
    public class HarmonyRoleClaim : IdentityRoleClaim<string>, IAuditableEntity<int>
    {
        public string Description { get; set; }
        public string Group { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public virtual HarmonyRole Role { get; set; }

        public HarmonyRoleClaim() : base()
        {
        }

        public HarmonyRoleClaim(string roleClaimDescription = null, string roleClaimGroup = null) : base()
        {
            Description = roleClaimDescription;
            Group = roleClaimGroup;
        }
    }
}