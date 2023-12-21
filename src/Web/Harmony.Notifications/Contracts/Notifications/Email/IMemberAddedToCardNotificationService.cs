using Harmony.Application.Notifications.Email;

namespace Harmony.Notifications.Contracts.Notifications.Email
{
    public interface IMemberAddedToCardNotificationService : INotificationService
    {
        Task Notify(MemberAddedToCardNotification notification);
    }
}
