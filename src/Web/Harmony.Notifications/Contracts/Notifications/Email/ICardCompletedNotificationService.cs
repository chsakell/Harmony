using Harmony.Application.Notifications;

namespace Harmony.Notifications.Contracts.Notifications.Email
{
    public interface ICardCompletedNotificationService : INotificationService
    {
        Task Notify(CardCompletedNotification notification);
    }
}
