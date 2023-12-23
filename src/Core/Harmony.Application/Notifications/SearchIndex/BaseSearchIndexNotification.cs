using Harmony.Domain.Enums;

namespace Harmony.Application.Notifications.SearchIndex
{
    public abstract class BaseSearchIndexNotification : ISearchIndexNotification
    {
        public abstract SearchIndexNotificationType Type { get; }
        public Guid BoardId { get; set; }

        public BaseSearchIndexNotification() { }
    }

    public interface ISearchIndexNotification
    {
        public abstract SearchIndexNotificationType Type { get; }
        Guid BoardId { get; set; }
    }
}
