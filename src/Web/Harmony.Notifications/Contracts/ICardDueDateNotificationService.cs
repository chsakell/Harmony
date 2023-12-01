namespace Harmony.Notifications.Contracts
{
    public interface ICardDueDateNotificationService : IJobNotificationService
    {
        Task SendCardDueDateChangedNotification(Guid cardId);
    }
}
