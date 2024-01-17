using Harmony.Application.Notifications.Email;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Domain.Enums;

namespace Harmony.Application.Contracts.Messaging
{
    public interface INotificationsPublisher
    {
        void PublishMessage<T>(T message, NotificationType type, string routingKey);
        void PublishEmailNotification<T>(T notification) where T : BaseEmailNotification;
        void PublishSearchIndexNotification<T>(T notification, string index) where T : BaseSearchIndexNotification;
    }
}
