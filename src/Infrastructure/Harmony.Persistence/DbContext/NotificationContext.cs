using Harmony.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Persistence.DbContext
{
    public class NotificationContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<Notification> Notifications { get; set; }

        public NotificationContext(DbContextOptions<NotificationContext> options) : base(options)
        {

        }
    }
}
