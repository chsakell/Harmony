using Harmony.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

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

            builder.HasMany(u => u.Comments)
                .WithOne()
                .HasForeignKey(u => u.UserId);

            builder.HasMany(u => u.CardActivities)
                .WithOne()
                .HasForeignKey(u => u.UserId);
        }
    }
}
