using Harmony.Application.Enums;

namespace Harmony.Application.Notifications
{
    public abstract class BaseNotification : INotification
    {
        public abstract NotificationType Type { get; }
    }

    public interface INotification
    {
        public abstract NotificationType Type { get; }
    }
}
