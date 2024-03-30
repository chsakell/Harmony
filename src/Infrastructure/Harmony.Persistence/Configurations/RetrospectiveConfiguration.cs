using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Harmony.Domain.Entities;

namespace Harmony.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for Retrospective
    /// </summary>
    public class RetrospectiveConfiguration : IEntityTypeConfiguration<Retrospective>
    {
        public void Configure(EntityTypeBuilder<Retrospective> builder)
        {
            builder.ToTable("Retrospectives");

            builder.Property(b => b.Name).IsRequired().HasMaxLength(300);

            builder.Property(b => b.BoardId).IsRequired();;

            builder.HasOne(retro => retro.ParentBoard)
                .WithMany(board => board.Retrospectives)
                .HasForeignKey(retro => retro.ParentBoardId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(retro => retro.Board)
                .WithOne(board => board.Retrospective)
                .HasForeignKey<Board>(board => board.RetrospectiveId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
