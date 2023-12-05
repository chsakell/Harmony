using Harmony.Application.Notifications;

namespace Harmony.Notifications.Contracts
{
    public interface IMemberAddedToBoardNotificationService : INotificationService
    {
        Task Notify(MemberAddedToBoardNotification notification);
    }
}
