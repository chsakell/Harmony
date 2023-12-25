using Harmony.Domain.Enums;
using System.Text.Json.Serialization;

namespace Harmony.Application.Notifications.SearchIndex
{
    public abstract class BaseSearchIndexNotification : ISearchIndexNotification
    {
        public abstract SearchIndexNotificationType Type { get; }
        public BaseSearchIndexNotification() { }
    }

    public interface ISearchIndexNotification
    {
        public abstract SearchIndexNotificationType Type { get; }
    }
}
