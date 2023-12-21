using Harmony.Application.Notifications;

namespace Harmony.Notifications.Contracts.Notifications.Email
{
    public interface IMemberRemovedFromBoardNotificationService : INotificationService
    {
        Task Notify(MemberRemovedFromBoardNotification notification);
    }
}
