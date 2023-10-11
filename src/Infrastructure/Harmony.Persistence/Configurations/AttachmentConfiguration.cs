using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Harmony.Domain.Entities;

namespace Harmony.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for CheckList Items
    /// </summary>
    public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
    {
        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.ToTable("Attachments");

            builder.Property(a => a.FileName).HasMaxLength(300).IsRequired();

            builder.Property(a => a.OriginalFileName).HasMaxLength(300).IsRequired();
        }
    }
}
