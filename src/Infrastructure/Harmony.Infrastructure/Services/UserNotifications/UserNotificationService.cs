using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.UserNotifications;
using Harmony.Application.Notifications.Email;

namespace Harmony.Infrastructure.Services.UserNotifications
{
    public class UserNotificationService : IUserNotificationService
    {
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IUserNotificationRepository _userNotificationRepository;

        public UserNotificationService(INotificationsPublisher notificationsPublisher,
            IUserNotificationRepository userNotificationRepository)
        {
            _notificationsPublisher = notificationsPublisher;
            _userNotificationRepository = userNotificationRepository;
        }

        public async Task HandleNotification<T>(string userId, T notification) where T : BaseEmailNotification
        {
            var subscribedToNotification = (await _userNotificationRepository
                .GetForUser(userId, notification.Type) != null);

            if (subscribedToNotification)
            {
                _notificationsPublisher.PublishEmailNotification(notification);
            }
        }
    }
}
