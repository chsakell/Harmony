using Harmony.Domain.Enums;

namespace Harmony.Application.Notifications
{
    public class CardDueTimeUpdatedNotification : BaseNotification
    {
        public Guid Id { get; set; }

        public CardDueTimeUpdatedNotification(Guid id)
        {
            Id = id;
        }

        public override NotificationType Type => NotificationType.CardDueDateUpdated;
    }
}
