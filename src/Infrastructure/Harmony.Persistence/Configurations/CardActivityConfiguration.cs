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
    /// EF Core entity configuration for Card Activities
    /// </summary>
    public class CardActivityConfiguration : IEntityTypeConfiguration<CardActivity>
    {
        public void Configure(EntityTypeBuilder<CardActivity> builder)
        {
            builder.ToTable("CardActivities");

            builder.Property(c => c.CardId).IsRequired();
            builder.Property(c => c.UserId).IsRequired();
            builder.Property(c => c.Activity).HasMaxLength(300).IsRequired();
        }
    }
}
