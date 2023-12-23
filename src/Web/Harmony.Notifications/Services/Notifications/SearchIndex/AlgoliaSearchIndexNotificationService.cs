using Algolia.Search.Clients;
using Harmony.Application.DTO.Search;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Notifications.Contracts.Notifications.SearchIndex;

namespace Harmony.Notifications.Services.Notifications.SearchIndex
{
    public class AlgoliaSearchIndexNotificationService : ISearchIndexNotificationService
    {
        private readonly ISearchClient _searchClient;

        public AlgoliaSearchIndexNotificationService(ISearchClient searchClient)
        {
            _searchClient = searchClient;
        }

        public async Task AddToIndex<T>(T notification) where T : class, ISearchIndexNotification
        {
            var index = _searchClient.InitIndex($"board-{notification.BoardId}");

            var result = await index.SaveObjectAsync(notification);
        }

        public async Task UpdateCard<T>(T notification) where T : class, ISearchIndexNotification
        {
            var index = _searchClient.InitIndex($"board-{notification.BoardId}");

            var result = await index.PartialUpdateObjectAsync(notification, createIfNotExists: true);
        }

        public bool CreateIndex(string name)
        {
            var index = _searchClient.InitIndex(name);

            return index != null;
        }
    }
}
