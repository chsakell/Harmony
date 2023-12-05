using Harmony.Application.Notifications;

namespace Harmony.Notifications.Contracts
{
    public interface IMemberRemovedFromCardNotificationService : INotificationService
    {
        Task Notify(MemberRemovedFromCardNotification notification);
    }
}
