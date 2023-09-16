using Harmony.Persistence.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
