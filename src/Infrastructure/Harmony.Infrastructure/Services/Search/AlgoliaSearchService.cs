using Algolia.Search.Clients;
using Algolia.Search.Models.Common;
using Algolia.Search.Models.Search;
using Harmony.Application.Contracts.Services.Search;
using Harmony.Application.DTO.Search;
using Harmony.Application.Features.Search.Queries.GlobalSearch;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Infrastructure.Services.Search
{
    public class AlgoliaSearchService : ISearchService
    {
        private readonly ISearchClient _searchClient;

        public AlgoliaSearchService(ISearchClient searchClient)
        {
            _searchClient = searchClient;
        }

        public async Task<List<IndexedCard>> Search(List<Guid> boards, string term)
        {
            var indexedBoards = await GetIndexedBoards(boards);

            if(!indexedBoards.Any())
            {
                return Enumerable.Empty<IndexedCard>().ToList();
            }

            var indexQueries = new List<QueryMultiIndices>();
            
            foreach (var boardId in indexedBoards)
            {
                indexQueries.Add(new QueryMultiIndices($"board-{boardId}", term));
            }

            MultipleQueriesRequest request = new MultipleQueriesRequest
            {
                Requests = indexQueries
            };

            var res = await _searchClient.MultipleQueriesAsync<IndexedCard>(request);

            return res.Results.SelectMany(r => r.Hits).ToList();
        }

        

        private async Task<List<string>> GetIndexedBoards(List<Guid> boards)
        {
            var boardIds = new List<string>();
            var indices = await _searchClient.ListIndicesAsync();

            if(indices == null)
            {
                return boardIds;
            }

            foreach(var index in indices.Items)
            {
                var board = index.Name.Replace("board-", string.Empty);
                boardIds.Add(board);
            }

            return boardIds;
        }
    }
}
