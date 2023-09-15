using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Harmony.Domain.Entities;

namespace Harmony.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for User Cards
    /// </summary>
    public class UserCardConfiguration : IEntityTypeConfiguration<UserCard>
    {
        public void Configure(EntityTypeBuilder<UserCard> builder)
        {
            builder.ToTable("UserCards");

            // composite primary key
            builder.HasKey(uc => new { uc.CardId, uc.UserId }); // M2M with intermediate table

            // M-M relationship is actuall 2 X 1-M relationships
            // from intermidiate table to the two tables
            // The second 1-M between Users && UserCards is defined to ApplicationUserConfiguration

            builder
                .HasOne(ub => ub.Card)
                .WithMany(c => c.Members)
                .HasForeignKey(uc => uc.CardId)
                .OnDelete(DeleteBehavior.ClientCascade);
        }
    }
}
