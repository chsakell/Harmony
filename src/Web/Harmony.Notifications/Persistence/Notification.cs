using Harmony.Application.Enums;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace Harmony.Notifications.Persistence
{
    public class Notification
    {
        public int Id { get; set; }
        public Guid? CardId { get; set; }
        public Guid? BoardId { get; set; }
        public NotificationType Type { get; set; }
        public string JobId { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
