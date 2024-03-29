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

            builder.Property(b => b.BoardId).IsRequired();
        }
    }
}
