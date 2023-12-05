using Harmony.Application.Notifications;

namespace Harmony.Notifications.Contracts
{
    public interface IMemberRemovedFromWorkspaceNotificationService : INotificationService
    {
        Task Notify(MemberRemovedFromWorkspaceNotification notification);
    }
}
