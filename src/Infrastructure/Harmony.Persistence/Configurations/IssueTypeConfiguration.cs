using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Harmony.Domain.Entities;

namespace Harmony.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for Issue Types
    /// </summary>
    public class IssueTypeConfiguration : IEntityTypeConfiguration<IssueType>
    {
        public void Configure(EntityTypeBuilder<IssueType> builder)
        {
            builder.ToTable("IssueTypes");

            builder.Property(b => b.Summary).HasMaxLength(20);
            builder.Property(b => b.Description).HasMaxLength(300);

            builder.HasMany(i => i.Cards)
                .WithOne(c => c.IssueType)
                .HasForeignKey(c => c.IssueTypeId)
                .IsRequired(false);
        }
    }
}
