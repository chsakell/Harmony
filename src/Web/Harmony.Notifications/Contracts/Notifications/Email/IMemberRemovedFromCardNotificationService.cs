using Harmony.Application.Notifications;

namespace Harmony.Notifications.Contracts.Notifications.Email
{
    public interface IMemberRemovedFromCardNotificationService : INotificationService
    {
        Task Notify(MemberRemovedFromCardNotification notification);
    }
}
