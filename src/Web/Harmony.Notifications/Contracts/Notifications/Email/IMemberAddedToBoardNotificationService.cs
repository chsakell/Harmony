using Harmony.Application.Notifications.Email;

namespace Harmony.Notifications.Contracts.Notifications.Email
{
    public interface IMemberAddedToBoardNotificationService : INotificationService
    {
        Task Notify(MemberAddedToBoardNotification notification);
    }
}
