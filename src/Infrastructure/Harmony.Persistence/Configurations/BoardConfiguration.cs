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

            builder.Property(b => b.Title).IsRequired().HasMaxLength(300);
            builder.Property(b => b.Description).IsRequired().HasMaxLength(100);

            builder.Property(b => b.Visibility).IsRequired();

            builder.Property(b => b.Key).IsRequired().HasMaxLength(5);

            // A board can have multiple lists and a list belongs to one board (1-M relationship)
            builder.HasMany(board => board.Lists)
                .WithOne(l => l.Board)
                .HasForeignKey(list => list.BoardId);

            builder.HasMany(board => board.Labels)
                .WithOne(label => label.Board)
                .HasForeignKey(label => label.BoardId);

            builder.HasMany(board => board.IssueTypes)
                .WithOne(label => label.Board)
                .HasForeignKey(label => label.BoardId);

            builder.HasMany(board => board.Sprints)
                .WithOne(label => label.Board)
                .HasForeignKey(label => label.BoardId);

            builder.Property(b => b.Type).IsRequired();

            builder.HasIndex(b => new { b.WorkspaceId, b.Key }).IsUnique();
        }
    }
}
