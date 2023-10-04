using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Harmony.Domain.Entities;

namespace Harmony.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for Board List
    /// </summary>
    public class LabelConfiguration : IEntityTypeConfiguration<Label>
    {
        public void Configure(EntityTypeBuilder<Label> builder)
        {
            builder.ToTable("Labels");

            builder.Property(b => b.Title).HasMaxLength(50);
            builder.Property(b => b.Colour).HasMaxLength(50).IsRequired();
            builder.Property(b => b.BoardId).IsRequired();
        }
    }
}
