using Algolia.Search.Clients;
using Algolia.Search.Models.Search;
using Harmony.Application.Contracts.Services.Search;
using Harmony.Application.DTO.Search;
using Harmony.Application.Features.Search.Queries.GlobalSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Infrastructure.Services.Search
{
    public class AngoliaSearchService : ISearchService
    {
        private readonly ISearchClient _searchClient;

        public AngoliaSearchService(ISearchClient searchClient)
        {
            _searchClient = searchClient;
        }

        public async Task<List<SearchableCard>> SearchBoard(Guid boardId, string term)
        {
            var index = _searchClient.InitIndex($"board-{boardId}");

            var result = await index.SearchAsync<SearchableCard>(new Query(term));

            return result.Hits;
        }

        public async Task AddCardToIndex(Guid boardId, SearchableCard card)
        {
            var index = _searchClient.InitIndex($"board-{boardId}");

            var result = await index.SaveObjectAsync(card);
        }

        public async Task UpdateCard(Guid boardId, SearchableCard card)
        {
            var index = _searchClient.InitIndex($"board-{boardId}");

            var result = await index.PartialUpdateObjectAsync(card);
        }

        public bool CreateIndex(string name)
        {
            var index = _searchClient.InitIndex(name);

            return index != null;
        }
    }
}
