using Algolia.Search.Clients;
using Algolia.Search.Models.Settings;
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

        public async Task AddToIndex<T>(T notification, string indexName) where T : class, ISearchIndexNotification
        {
            var index = _searchClient.InitIndex(indexName);

            var result = await index.SaveObjectAsync(notification);
        }

        public async Task UpdateCard<T>(T notification, string indexName) where T : class, ISearchIndexNotification
        {
            var index = _searchClient.InitIndex(indexName);

            var result = await index.PartialUpdateObjectAsync(notification, createIfNotExists: true);
        }

        public async Task CreateIndex<T>(T notification, string indexName) where T : class, ISearchIndexNotification
        {
            if(notification is BoardCreatedIndexNotification boardCreatedNotification)
            {
                var index = _searchClient.InitIndex(indexName);
                var settings = new IndexSettings
                {
                    SearchableAttributes = boardCreatedNotification.SearchableAttributes,
                    AttributesForFaceting = boardCreatedNotification.AttributesForFaceting
                };

                await index.SetSettingsAsync(settings);
            }
        }
    }
}
