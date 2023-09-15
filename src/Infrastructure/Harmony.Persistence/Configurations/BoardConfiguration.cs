using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Harmony.Domain.Entities;

namespace Harmony.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for Board List
    /// </summary>
    public class BoardConfiguration : IEntityTypeConfiguration<Board>
    {
        public void Configure(EntityTypeBuilder<Board> builder)
        {
            builder.ToTable("Boards");

            builder.Property(b => b.UserId).IsRequired();

            builder.Property(b => b.Name).IsRequired().HasMaxLength(300);

            builder.Property(b => b.Visibility).IsRequired();

            // A board can have multiple lists and a list belongs to one board (1-M relationship)
            builder.HasMany(board => board.Lists)
                .WithOne(l => l.Board)
                .HasForeignKey(list => list.BoardId);

            builder.HasMany(board => board.Labels)
                .WithOne(label => label.Board)
                .HasForeignKey(label => label.BoardId);
        }
    }
}
