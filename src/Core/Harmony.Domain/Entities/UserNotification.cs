using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Domain.Entities
{
    public class UserNotification : AuditableEntity<Guid>
    {
        public string UserId { get; set; }
        public NotificationType NotificationType { get; set; }
    }
}
