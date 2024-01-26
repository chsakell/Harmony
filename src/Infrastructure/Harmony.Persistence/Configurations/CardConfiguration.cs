using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

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

            builder.Property(c => c.BoardListId).IsRequired(false);

            builder.Property(c => c.Status).IsRequired().HasDefaultValue(CardStatus.Active);

            builder.Property(c => c.Title).IsRequired().HasMaxLength(300);

            builder.Property(c => c.Position).IsRequired();

            builder.Property(c => c.DueDateReminderType).IsRequired(false).HasDefaultValue(DueDateReminderType.None);

            builder.Property(c => c.StoryPoints).IsRequired(false);

            builder.Property(c => c.DateCompleted).IsRequired(false);

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

            builder.HasMany(c => c.Attachments)
                .WithOne(a => a.Card)
                .HasForeignKey(a => a.CardId);

            builder.HasQueryFilter(c => !c.ParentCardId.HasValue);

            builder.HasOne(x => x.ParentCard)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentCardId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
