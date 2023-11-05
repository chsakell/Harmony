using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Harmony.Persistence.Identity;

namespace Harmony.Persistence.Configurations.Identity
{
    /// <summary>
    /// EF Core entity configuration for Harmony User
    /// </summary>
    public class HarmonyUserConfiguration : IEntityTypeConfiguration<HarmonyUser>
    {
        public void Configure(EntityTypeBuilder<HarmonyUser> builder)
        {
            builder.ToTable("Users", "identity");

            builder.Property(u => u.ProfilePicture).HasColumnType("text");

            // A user can create multiple workspaces and a board belongs to one user (1-M relationship)
            builder.HasMany(user => user.Workspaces)
                .WithOne()
                .HasForeignKey(workspace => workspace.UserId).IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            // extra configuration to fullfill the M2M relationship for access,
            // between users and boards
            builder
                .HasMany(user => user.AccessWorkspaces)
                .WithOne()
                .HasForeignKey(userWorkspace => userWorkspace.UserId).IsRequired();

            // A user can create multiple boards and a board belongs to one user (1-M relationship)
            builder.HasMany(user => user.Boards)
                .WithOne()
                .HasForeignKey(board => board.UserId).IsRequired();

            // extra configuration to fullfill the M2M relationship for access,
            // between users and boards
            builder
                .HasMany(user => user.AccessBoards)
                .WithOne()
                .HasForeignKey(board => board.UserId);

			// A user can create multiple board lists and a list belongs to one user (1-M relationship)
			builder.HasMany(user => user.BoardLists)
				.WithOne()
				.HasForeignKey(board => board.UserId).IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

			// A user can create multiple cards and a card belongs to one user (1-M relationship)
			builder.HasMany(user => user.Cards)
				.WithOne()
				.HasForeignKey(board => board.UserId).IsRequired()
				.OnDelete(DeleteBehavior.NoAction);

			// extra configuration to fullfill the M2M relationship for access,
			// between users and cards
			builder
                .HasMany(user => user.AccessCards)
                .WithOne()
                .HasForeignKey(board => board.UserId).IsRequired();

            // A user can create multiple checklists and a board belongs to one user (1-M relationship)
            builder.HasMany(user => user.CheckLists)
                .WithOne()
                .HasForeignKey(board => board.UserId).IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(u => u.Comments)
                .WithOne()
                .HasForeignKey(u => u.UserId);

            builder.HasMany(u => u.CardActivities)
                .WithOne()
                .HasForeignKey(u => u.UserId);
        }
    }
}
