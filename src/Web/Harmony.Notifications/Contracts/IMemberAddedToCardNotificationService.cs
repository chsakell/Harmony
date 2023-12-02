using Harmony.Application.Notifications;

namespace Harmony.Notifications.Contracts
{
    public interface IMemberAddedToCardNotificationService : INotificationService
    {
        Task Notify(MemberAddedToCardNotification notification);
    }
}
