namespace Harmony.Notifications.Contracts
{
    public interface ICardCompletedNotificationService : INotificationService
    {
        Task SendCardCompletedNotification(Guid cardId);
    }
}
