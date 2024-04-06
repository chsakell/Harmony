using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Harmony.Domain.Entities;
using Harmony.Persistence.Converters;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Harmony.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for Links
    /// </summary>
    public class LinkConfiguration : IEntityTypeConfiguration<Link>
    {
        public void Configure(EntityTypeBuilder<Link> builder)
        {
            builder.ToTable("Links");

            builder.Property(l => l.SourceCardId).IsRequired(true);
            builder.Property(l => l.TargetCardId).IsRequired(true);
            builder.Property(l => l.UserId).IsRequired(true);
        }
    }
}
