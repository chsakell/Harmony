using Harmony.Application.Notifications.Email;
using Harmony.Application.Notifications.SearchIndex;

namespace Harmony.Application.Contracts.Messaging
{
    public interface INotificationsPublisher
    {
        void PublishEmailNotification<T>(T notification) where T : BaseEmailNotification;
        void PublishSearchIndexNotification<T>(T notification) where T : BaseSearchIndexNotification;
    }
}
