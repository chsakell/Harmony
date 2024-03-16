using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

namespace Harmony.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for Workspace
    /// </summary>
    public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
    {
        public void Configure(EntityTypeBuilder<Workspace> builder)
        {
            builder.ToTable("Workspaces");

            builder.Property(w => w.Name).HasMaxLength(50).IsRequired();

            builder.Property(w => w.Description).HasMaxLength(500);

            builder.Property(w => w.Status).IsRequired().HasDefaultValue(WorkspaceStatus.Active);

            builder.HasQueryFilter(w => w.Status == WorkspaceStatus.Active);

            // A Workspace can have multiple boards and a board belongs to one Workspace (1-M relationship)
            builder.HasMany(w => w.Boards)
                .WithOne(b => b.Workspace)
                .HasForeignKey(b => b.WorkspaceId);
        }
    }
}
