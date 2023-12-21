using Harmony.Application.Notifications;

namespace Harmony.Notifications.Contracts.Notifications.Email
{
    public interface IMemberAddedToWorkspaceNotificationService : INotificationService
    {
        Task Notify(MemberAddedToWorkspaceNotification notification);
    }
}
