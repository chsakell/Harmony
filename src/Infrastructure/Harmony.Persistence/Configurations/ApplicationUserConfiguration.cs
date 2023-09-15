using Harmony.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for Board
    /// </summary>
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // A user can create multiple boards and a board belongs to one user (1-M relationship)
            builder.HasMany(user => user.Boards)
                .WithOne()
                .HasForeignKey(board => board.UserId);

            // extra configuration to fullfill the M2M relationship for access,
            // between users and boards
            builder
                .HasMany(user => user.AccessBoards)
                .WithOne()
                .HasForeignKey(board => board.UserId);

            // extra configuration to fullfill the M2M relationship for access,
            // between users and cards
            builder
                .HasMany(user => user.AccessCards)
                .WithOne()
                .HasForeignKey(board => board.UserId);

            builder.HasMany(c => c.Comments)
                .WithOne()
                .HasForeignKey(c => c.UserId);
        }
    }
}
