using Harmony.Application.Notifications.Email;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Domain.Enums;

namespace Harmony.Application.Contracts.Messaging
{
    public interface INotificationsPublisher
    {
        Task PublishMessageAsync<T>(T message, NotificationType type, string routingKey);
        Task PublishEmailNotificationAsync<T>(T notification) where T : BaseEmailNotification;
        Task PublishSearchIndexNotificationAsync<T>(T notification, string index) where T : BaseSearchIndexNotification;
    }
}
