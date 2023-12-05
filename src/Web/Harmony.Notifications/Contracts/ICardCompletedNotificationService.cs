using Harmony.Application.Notifications;

namespace Harmony.Notifications.Contracts
{
    public interface ICardCompletedNotificationService : INotificationService
    {
        Task Notify(CardCompletedNotification notification);
    }
}
