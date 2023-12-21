using Harmony.Application.Notifications.Email;

namespace Harmony.Notifications.Contracts.Notifications.Email
{
    public interface IMemberRemovedFromWorkspaceNotificationService : INotificationService
    {
        Task Notify(MemberRemovedFromWorkspaceNotification notification);
    }
}
