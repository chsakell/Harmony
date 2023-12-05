using Harmony.Application.Notifications;

namespace Harmony.Notifications.Contracts
{
    public interface ICardDueDateNotificationService : INotificationService
    {
        Task Notify(CardDueTimeUpdatedNotification notification);
    }
}
