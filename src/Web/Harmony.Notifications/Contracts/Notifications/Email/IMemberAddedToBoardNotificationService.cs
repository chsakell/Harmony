using Harmony.Application.Notifications;

namespace Harmony.Notifications.Contracts.Notifications.Email
{
    public interface IMemberAddedToBoardNotificationService : INotificationService
    {
        Task Notify(MemberAddedToBoardNotification notification);
    }
}
