using Harmony.Application.Notifications;

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
