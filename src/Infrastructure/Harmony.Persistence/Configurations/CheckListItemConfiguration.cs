using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Harmony.Domain.Entities;

namespace Harmony.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for CheckList Items
    /// </summary>
    public class CheckListItemConfiguration : IEntityTypeConfiguration<CheckListItem>
    {
        public void Configure(EntityTypeBuilder<CheckListItem> builder)
        {
            builder.ToTable("CheckListItems");

            builder.Property(c => c.Description).IsRequired();
            builder.Property(c => c.CheckListId).IsRequired();
            builder.Property(c => c.IsChecked).IsRequired().HasDefaultValue(false);
            builder.Property(c => c.Position).IsRequired();
        }
    }
}
