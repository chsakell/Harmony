using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Harmony.Domain.Entities;

namespace Harmony.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for Sprints
    /// </summary>
    public class SprintConfiguration : IEntityTypeConfiguration<Sprint>
    {
        public void Configure(EntityTypeBuilder<Sprint> builder)
        {
            builder.ToTable("Sprints");

            builder.Property(b => b.Name).HasMaxLength(50);
            builder.Property(b => b.Goal).HasMaxLength(200).IsRequired(false);
            builder.Property(b => b.BoardId).IsRequired();
            builder.Property(b => b.StartDate).IsRequired(false);
            builder.Property(b => b.EndDate).IsRequired(false);

            builder.HasMany(sprint => sprint.Cards)
                .WithOne(c => c.Sprint)
                .HasForeignKey(card => card.SprintId)
                .IsRequired(false);
        }
    }
}
