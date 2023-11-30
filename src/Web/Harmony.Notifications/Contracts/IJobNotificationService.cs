namespace Harmony.Notifications.Contracts
{
    public interface IJobNotificationService
    {
        Task SendCardDueDateChangedNotification(Guid cardId);
    }
}
