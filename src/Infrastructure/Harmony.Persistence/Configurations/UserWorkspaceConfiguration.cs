using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Harmony.Domain.Entities;

namespace Harmony.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for User Workspaces
    /// </summary>
    public class UserWorkspaceConfiguration : IEntityTypeConfiguration<UserWorkspace>
    {
        public void Configure(EntityTypeBuilder<UserWorkspace> builder)
        {
            builder.ToTable("UserWorkspaces");

            // composite primary key
            builder.HasKey(ub => new { ub.WorkspaceId, ub.UserId }); // M2M with intermediate table

            // M-M relationship is actuall 2 X 1-M relationships
            // from intermidiate table to the two tables
            // The second 1-M between Users && UserWorkspaces is defined to HarmonyUserConfiguration

            builder
                .HasOne(ub => ub.Workspace)
                .WithMany(b => b.Users)
                .HasForeignKey(ub => ub.WorkspaceId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
