using Harmony.Application.Notifications.Email;

namespace Harmony.Notifications.Contracts.Notifications.Email
{
    public interface IMemberAddedToWorkspaceNotificationService : INotificationService
    {
        Task Notify(MemberAddedToWorkspaceNotification notification);
    }
}
