using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Harmony.Domain.Entities;
using Harmony.Persistence.Converters;

namespace Harmony.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for User Notifications
    /// </summary>
    public class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
    {
        public void Configure(EntityTypeBuilder<UserNotification> builder)
        {
            builder.ToTable("UserNotifications");

            // composite primary key
            builder.Property(un => un.UserId).IsRequired();
            builder.Property(un => un.NotificationType).IsRequired();
            builder.Property(un => un.NotificationType).HasConversion<NotificationTypeConverter>();
            builder.Property(un =>un.UserId).IsRequired();
        }
    }
}
