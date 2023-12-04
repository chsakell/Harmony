using Harmony.Domain.Enums;

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
