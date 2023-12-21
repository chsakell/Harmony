using Harmony.Application.Notifications.Email;

namespace Harmony.Application.Contracts.Messaging
{
    public interface INotificationsPublisher
    {
        void Publish<T>(T notification) where T : BaseEmailNotification;
    }
}
