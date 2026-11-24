using Harmony.Domain.Enums;

namespace Harmony.Application.Notifications.SearchIndex
{
    public abstract class BaseSearchIndexNotification : ISearchIndexNotification
    {
        public abstract SearchIndexNotificationType Type { get; }
        public abstract string ObjectID { get; set; }
        public BaseSearchIndexNotification() { }
    }

    public interface ISearchIndexNotification
    {
        public abstract SearchIndexNotificationType Type { get; }

        /// <summary>
        /// Search engine document id (Algolia objectID).
        /// </summary>
        string ObjectID { get; }
    }
}
