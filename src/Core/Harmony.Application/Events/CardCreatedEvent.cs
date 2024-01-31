using Harmony.Application.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Events
{
    public class CardCreatedEvent
    {
        public CardCreatedMessage Message { get; set; }
        public CardCreatedEvent(CardCreatedMessage message)
        {
            Message = message;
        }
    }
}
