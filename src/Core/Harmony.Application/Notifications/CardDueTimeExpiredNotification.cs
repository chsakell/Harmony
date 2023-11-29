using Harmony.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Notifications
{
    public class CardDueTimeExpiredNotification : BaseNotification
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }

        public override NotificationType Type => NotificationType.CardChangedDueDate;
    }
}
