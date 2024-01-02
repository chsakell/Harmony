using Harmony.Application.Notifications.Email;

namespace Harmony.Application.Contracts.Services.UserNotifications
{
    public interface IUserNotificationService
    {
        Task HandleNotification<T>(string userId, T notification) where T : BaseEmailNotification;
    }
}
