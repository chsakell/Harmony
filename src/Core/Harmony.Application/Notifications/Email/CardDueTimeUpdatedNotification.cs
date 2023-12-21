using Harmony.Domain.Enums;

namespace Harmony.Application.Notifications.Email
{
    public class CardDueTimeUpdatedNotification : BaseEmailNotification
    {
        public Guid Id { get; set; }

        public CardDueTimeUpdatedNotification(Guid id)
        {
            Id = id;
        }

        public override EmailNotificationType Type => EmailNotificationType.CardDueDateUpdated;
    }
}
