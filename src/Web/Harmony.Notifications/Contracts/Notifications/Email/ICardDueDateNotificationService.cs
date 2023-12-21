using Harmony.Application.Notifications;

namespace Harmony.Notifications.Contracts.Notifications.Email
{
    public interface ICardDueDateNotificationService : INotificationService
    {
        Task Notify(CardDueTimeUpdatedNotification notification);
    }
}
