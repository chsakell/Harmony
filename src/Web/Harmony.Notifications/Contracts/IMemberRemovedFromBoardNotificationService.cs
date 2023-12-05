using Harmony.Application.Notifications;

namespace Harmony.Notifications.Contracts
{
    public interface IMemberRemovedFromBoardNotificationService : INotificationService
    {
        Task Notify(MemberRemovedFromBoardNotification notification);
    }
}
