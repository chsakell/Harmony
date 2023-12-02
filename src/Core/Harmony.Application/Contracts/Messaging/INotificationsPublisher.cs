using Harmony.Application.Notifications;

namespace Harmony.Application.Contracts.Messaging
{
    public interface INotificationsPublisher
    {
        void Publish<T>(T notification) where T : BaseNotification;
    }
}
