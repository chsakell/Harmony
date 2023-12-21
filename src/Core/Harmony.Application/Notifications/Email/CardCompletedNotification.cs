using Harmony.Domain.Enums;

namespace Harmony.Application.Notifications.Email
{
    public class CardCompletedNotification : BaseEmailNotification
    {
        public override EmailNotificationType Type => EmailNotificationType.CardCompleted;

        public Guid Id { get; set; }

        public CardCompletedNotification(Guid id)
        {
            Id = id;
        }
    }
}
