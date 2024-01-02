using Harmony.Application.Notifications.SearchIndex;

namespace Harmony.Notifications.Contracts.Notifications.SearchIndex
{
    public interface ISearchIndexNotificationService
    {
        Task CreateIndex<T>(T notification, string indexName) where T : class, ISearchIndexNotification;
        Task AddToIndex<T>(T notification, string indexName) where T : class, ISearchIndexNotification;
        Task UpdateCard<T>(T notification, string indexName) where T : class, ISearchIndexNotification;
    }
}
