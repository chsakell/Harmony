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
    /// EF Core entity configuration for Board List
    /// </summary>
    public class BoardListConfiguration : IEntityTypeConfiguration<BoardList>
    {
        public void Configure(EntityTypeBuilder<BoardList> builder)
        {
            builder.ToTable("BoardLists");

            builder.Property(b => b.BoardId).IsRequired();

            builder.Property(b => b.Name).IsRequired().HasMaxLength(300);

            builder.Property(b => b.Position).IsRequired();
        }
    }
}
