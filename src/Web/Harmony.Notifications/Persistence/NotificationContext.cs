using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Notifications.Persistence
{
    public class NotificationContext : DbContext
    {
        public DbSet<Notification> Notifications { get; set; }

        public NotificationContext(DbContextOptions<NotificationContext> options) : base(options)
        {

        }
    }
}
