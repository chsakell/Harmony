using Harmony.Application.Notifications;

namespace Harmony.Notifications.Contracts.Notifications.Email
{
    public interface IMemberAddedToCardNotificationService : INotificationService
    {
        Task Notify(MemberAddedToCardNotification notification);
    }
}
