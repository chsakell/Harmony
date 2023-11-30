using Harmony.Application.Enums;

namespace Harmony.Application.Notifications
{
    public class CardDueTimeExpiredNotification : BaseNotification
    {
        public Guid Id { get; set; }

        public CardDueTimeExpiredNotification(Guid id)
        {
            Id = id;
        }

        public override NotificationType Type => NotificationType.CardChangedDueDate;
    }
}
