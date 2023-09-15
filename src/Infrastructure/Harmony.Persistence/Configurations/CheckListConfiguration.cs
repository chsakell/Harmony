using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Harmony.Domain.Entities;

namespace Harmony.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for CheckLists
    /// </summary>
    public class CheckListConfiguration : IEntityTypeConfiguration<CheckList>
    {
        public void Configure(EntityTypeBuilder<CheckList> builder)
        {
            builder.ToTable("CheckLists");

            builder.Property(c => c.Title).IsRequired();
            builder.Property(c => c.CardId).IsRequired();
            builder.Property(c => c.Position).IsRequired();

            builder.HasMany(c => c.Items)
                .WithOne(i => i.CheckList)
                .HasForeignKey(i => i.CheckListId);
        }
    }
}
