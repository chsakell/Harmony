using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

namespace Harmony.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for User Boards
    /// </summary>
    public class UserBoardConfiguration : IEntityTypeConfiguration<UserBoard>
    {
        public void Configure(EntityTypeBuilder<UserBoard> builder)
        {
            builder.ToTable("UserBoards");

            // composite primary key
            builder.HasKey(ub => new { ub.BoardId, ub.UserId }); // M2M with intermediate table

            // M-M relationship is actuall 2 X 1-M relationships
            // from intermidiate table to the two tables
            // The second 1-M between Users && UserBoards is defined to ApplicationUserConfiguration

            builder.Property(ub => ub.Access).HasDefaultValue(UserBoardAccess.Member);

            builder
                .HasOne(ub => ub.Board)
                .WithMany(b => b.Users)
                .HasForeignKey(ub => ub.BoardId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
