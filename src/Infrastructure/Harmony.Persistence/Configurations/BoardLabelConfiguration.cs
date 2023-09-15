using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Harmony.Domain.Entities;

namespace Harmony.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for Board List
    /// </summary>
    public class BoardLabelConfiguration : IEntityTypeConfiguration<BoardLabel>
    {
        public void Configure(EntityTypeBuilder<BoardLabel> builder)
        {
            builder.ToTable("BoardBoardLabels");

            builder.Property(b => b.Name).HasMaxLength(100).IsRequired();
            builder.Property(b => b.Colour).HasMaxLength(50).IsRequired();
            builder.Property(b => b.BoardId).IsRequired();
        }
    }
}
