using Harmony.Application.Notifications.Email;

namespace Harmony.Notifications.Contracts.Notifications.Email
{
    public interface IMemberRemovedFromBoardNotificationService : INotificationService
    {
        Task Notify(MemberRemovedFromBoardNotification notification);
    }
}
