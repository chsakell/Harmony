namespace Harmony.Notifications.Contracts
{
    public interface ICardDueDateNotificationService : INotificationService
    {
        Task SendCardDueDateChangedNotification(Guid cardId);
    }
}
