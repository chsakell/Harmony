using Harmony.Domain.Enums;

namespace Harmony.Domain.Entities
{
    public class UserNotification : AuditableEntity<Guid>
    {
        public string UserId { get; set; }
        public EmailNotificationType NotificationType { get; set; }
    }
}
