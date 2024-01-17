using Harmony.Application.Notifications.Email;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Domain.Enums;

namespace Harmony.Application.Contracts.Messaging
{
    public interface INotificationsPublisher
    {
        void PublishNotification<T>(T notification, NotificationType type);
        void PublishEmailNotification<T>(T notification) where T : BaseEmailNotification;
        void PublishSearchIndexNotification<T>(T notification, string index) where T : BaseSearchIndexNotification;
    }
}
