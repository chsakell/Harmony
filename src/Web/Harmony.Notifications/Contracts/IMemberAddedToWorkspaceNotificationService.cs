using Harmony.Application.Notifications;

namespace Harmony.Notifications.Contracts
{
    public interface IMemberAddedToWorkspaceNotificationService : INotificationService
    {
        Task Notify(MemberAddedToWorkspaceNotification notification);
    }
}
