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
