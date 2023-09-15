using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony.Domain.Entities;

namespace Harmony.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for Cards
    /// </summary>
    public class CardConfiguration : IEntityTypeConfiguration<Card>
    {
        public void Configure(EntityTypeBuilder<Card> builder)
        {
            builder.ToTable("Cards");

            builder.Property(c => c.BoardListId).IsRequired();

            builder.Property(c => c.IsArchived).IsRequired().HasDefaultValue(false);

            builder.Property(c => c.Name).IsRequired().HasMaxLength(300);

            builder.Property(c => c.Position).IsRequired();

            builder.HasMany(c => c.Comments)
                .WithOne(c => c.Card)
                .HasForeignKey(c => c.CardId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(c => c.CheckLists)
                .WithOne(c => c.Card)
                .HasForeignKey(c => c.CardId);

            builder.HasMany(c => c.Activities)
                .WithOne(c => c.Card)
                .HasForeignKey(c => c.CardId)
                .OnDelete(DeleteBehavior.ClientCascade);
        }
    }
}
