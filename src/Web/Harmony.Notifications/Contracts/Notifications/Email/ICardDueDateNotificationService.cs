using Harmony.Application.Notifications.Email;

namespace Harmony.Notifications.Contracts.Notifications.Email
{
    public interface ICardDueDateNotificationService : INotificationService
    {
        Task Notify(CardDueTimeUpdatedNotification notification);
    }
}
