using Harmony.Persistence.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Persistence.Configurations.Identity
{
    /// <summary>
    /// EF Core entity configuration for Harmony Role Claim
    /// </summary>
    public class HarmonyRoleClaimConfiguration : IEntityTypeConfiguration<HarmonyRoleClaim>
    {
        public void Configure(EntityTypeBuilder<HarmonyRoleClaim> builder)
        {
            builder.ToTable("RoleClaims", "identity");

            builder.HasOne(d => d.Role)
                    .WithMany(p => p.RoleClaims)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
