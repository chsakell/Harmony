using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Harmony.Domain.Entities;

namespace Harmony.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for Card Labels
    /// </summary>
    public class CardLabelConfiguration : IEntityTypeConfiguration<CardLabel>
    {
        public void Configure(EntityTypeBuilder<CardLabel> builder)
        {
            builder.ToTable("CardLabels");

            // composite primary key
            builder.HasKey(cardLabel => new { cardLabel.CardId, cardLabel.LabelId}); // M2M with intermediate table

            // M-M relationship is actuall 2 X 1-M relationships
            // from intermidiate table to the two tables

            builder
                .HasOne(cardLabel => cardLabel.Card)
                .WithMany(card => card.Labels)
                .HasForeignKey(cardLabel => cardLabel.CardId)
                .OnDelete(DeleteBehavior.ClientCascade);

            builder
                .HasOne(cardLabel => cardLabel.Label)
                .WithMany(boardLabel => boardLabel.Labels)
                .HasForeignKey(cardLabel => cardLabel.LabelId);
        }
    }
}
