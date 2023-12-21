using Harmony.Application.Notifications.Email;

namespace Harmony.Notifications.Contracts.Notifications.Email
{
    public interface IMemberRemovedFromCardNotificationService : INotificationService
    {
        Task Notify(MemberRemovedFromCardNotification notification);
    }
}
