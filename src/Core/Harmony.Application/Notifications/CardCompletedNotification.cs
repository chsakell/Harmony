using Harmony.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Notifications
{
    public class CardCompletedNotification : BaseNotification
    {
        public override NotificationType Type => NotificationType.CardCompleted;

        public Guid Id { get; set; }

        public CardCompletedNotification(Guid id)
        {
            Id = id;
        }
    }
}
