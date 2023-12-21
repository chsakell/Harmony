using Harmony.Domain.Enums;

namespace Harmony.Application.Notifications.Email
{
    public abstract class BaseEmailNotification : IEmailNotification
    {
        public abstract EmailNotificationType Type { get; }
    }

    public interface IEmailNotification
    {
        public abstract EmailNotificationType Type { get; }
    }
}
