using Harmony.Persistence.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Persistence.Configurations.Identity
{

    /// <summary>
    /// EF Core entity configuration for Harmony Role
    /// </summary>
    public class HarmonyRoleConfiguration : IEntityTypeConfiguration<HarmonyRole>
    {
        public void Configure(EntityTypeBuilder<HarmonyRole> builder)
        {
            builder.ToTable("Roles", "identity");
        }
    }
}
