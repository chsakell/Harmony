using Harmony.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Persistence.DbContext
{
    public class AutomationContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<Notification> Tasks { get; set; }

        public AutomationContext(DbContextOptions<AutomationContext> options) : base(options)
        {

        }
    }
}
