using Algolia.Search.Clients;
using Algolia.Search.Models.Common;
using Algolia.Search.Models.Search;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Contracts.Services.Search;
using Harmony.Application.DTO.Search;
using Harmony.Application.Features.Search.Commands.AdvancedSearch;
using Harmony.Application.Models;
using System.Text;

namespace Harmony.Infrastructure.Services.Search
{
    public class AlgoliaSearchService : ISearchService
    {
        private readonly ISearchClient _searchClient;
        private readonly IBoardService _boardService;

        public AlgoliaSearchService(ISearchClient searchClient, IBoardService boardService)
        {
            _searchClient = searchClient;
            _boardService = boardService;
        }

        public async Task<List<SearchableCard>> Search(List<Guid> boards, string term)
        {
            var result = new List<SearchableCard>();

            List<BoardInfo> boardInfos = new List<BoardInfo>();
            foreach (var boardId in boards)
            {
                var boardInfo = await _boardService.GetBoardInfo(boardId);
                if (boardInfo != null)
                {
                    boardInfos.Add(boardInfo);
                }
            }

            var indexedBoards = await GetIndexedBoards(boardInfos.Select(bi => bi.IndexName).ToList());

            if (!indexedBoards.Any())
            {
                return Enumerable.Empty<SearchableCard>().ToList();
            }

            var indexQueries = new List<QueryMultiIndices>();

            foreach (var index in indexedBoards)
            {
                indexQueries.Add(new QueryMultiIndices(index, term)
                {
                    RestrictSearchableAttributes = new List<string> { "title", "serialKey" }
                });
            }

            MultipleQueriesRequest request = new MultipleQueriesRequest
            {
                Requests = indexQueries
            };

            var multiQueryResult = await _searchClient.MultipleQueriesAsync<IndexedCard>(request);

            var hits = multiQueryResult.Results.SelectMany(x => x.Hits);

            foreach (var indexedCard in hits)
            {
                var searchableCard = new SearchableCard()
                {
                    CardId = indexedCard.ObjectID,
                    Title = indexedCard.Title,
                    IssueType = indexedCard.IssueType,
                    Status = indexedCard.Status,
                    SerialKey = indexedCard.SerialKey,
                    BoardId = indexedCard.BoardId
                };

                var board = boardInfos.FirstOrDefault(bi => bi.Id == indexedCard.BoardId);

                if (board != null)
                {
                    searchableCard.BoardTitle = board.Title;
                    searchableCard.BoardId = board.Id;

                    var list = board.Lists.FirstOrDefault(l => l.Id == indexedCard.ListId);

                    if (list != null)
                    {
                        searchableCard.List = list.Title;

                        searchableCard.IsComplete = list.CardStatus == Domain.Enums.BoardListCardStatus.DONE;
                    }
                }

                result.Add(searchableCard);
            }

            return result.OrderBy(card => card.SerialKey).ToList();
        }

        public async Task<List<SearchableCard>> Search(List<Guid> boards, AdvancedSearchCommand query)
        {
            return query.CombineFilters ? await AdvancedSearchCombined(boards, query) :
                await AdvancedSearch(boards, query);
        }

        private async Task<List<SearchableCard>> AdvancedSearchCombined(List<Guid> boards, AdvancedSearchCommand query)
        {
            var result = new List<SearchableCard>();

            List<BoardInfo> boardInfos = new List<BoardInfo>();
            foreach (var boardId in boards)
            {
                var boardInfo = await _boardService.GetBoardInfo(boardId);
                if (boardInfo != null)
                {
                    boardInfos.Add(boardInfo);
                }
            }

            var indexedBoards = await GetIndexedBoards(boardInfos.Select(bi => bi.IndexName).ToList());

            if (!indexedBoards.Any())
            {
                return Enumerable.Empty<SearchableCard>().ToList();
            }

            var indexQueries = new List<QueryMultiIndices>();

            var queryBuilder = new StringBuilder();
            var filterBuilder = new StringBuilder();
            var restrictSearchableAttributes = new List<string>();

            if (!string.IsNullOrEmpty(query.Title))
            {
                restrictSearchableAttributes.Add("title");
                BuildQuery(queryBuilder, query.Title);
            }

            if (query.ListId.HasValue)
            {
                restrictSearchableAttributes.Add("listId");
                BuildQuery(queryBuilder, query.ListId.Value);
            }

            if (!string.IsNullOrEmpty(query.Description))
            {
                restrictSearchableAttributes.Add("description");
                BuildQuery(queryBuilder, query.Description);
            }

            if (query.HasAttachments)
            {
                restrictSearchableAttributes.Add("hasAttachments");
                BuildFilter(filterBuilder, "hasAttachments:true");
            }

            if (query.DueDate.HasValue)
            {
                var startDate = query.DueDate.Value.Date;
                DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                long unixStartDateTime = (long)(startDate - sTime).TotalSeconds;

                var endDate = startDate.AddDays(1);
                long unixEndDateTime = (long)(endDate - sTime).TotalSeconds;

                restrictSearchableAttributes.Add("dueDate");
                BuildFilter(filterBuilder, $"dueDate:{unixStartDateTime} TO {unixEndDateTime}");
            }

            foreach (var index in indexedBoards)
            {
                indexQueries.Add(new QueryMultiIndices(index, queryBuilder.ToString())
                {
                    RestrictSearchableAttributes = restrictSearchableAttributes,
                    Filters = filterBuilder.ToString()
                });
            }

            MultipleQueriesRequest request = new MultipleQueriesRequest
            {
                Requests = indexQueries
            };

            var multiQueryResult = await _searchClient.MultipleQueriesAsync<IndexedCard>(request);

            var hits = multiQueryResult.Results.SelectMany(x => x.Hits);

            foreach (var indexedCard in hits)
            {
                var searchableCard = new SearchableCard()
                {
                    CardId = indexedCard.ObjectID,
                    Title = indexedCard.Title,
                    IssueType = indexedCard.IssueType,
                    Status = indexedCard.Status,
                    SerialKey = indexedCard.SerialKey,
                    BoardId = indexedCard.BoardId
                };

                var board = boardInfos.FirstOrDefault(bi => bi.Id == indexedCard.BoardId);

                if (board != null)
                {
                    searchableCard.BoardTitle = board.Title;
                    searchableCard.BoardId = board.Id;

                    var list = board.Lists.FirstOrDefault(l => l.Id == indexedCard.ListId);

                    if (list != null)
                    {
                        searchableCard.List = list.Title;

                        searchableCard.IsComplete = list.CardStatus == Domain.Enums.BoardListCardStatus.DONE;
                    }
                }

                result.Add(searchableCard);
            }

            return result.DistinctBy(card => card.CardId).OrderBy(card => card.SerialKey).ToList();
        }

        private async Task<List<SearchableCard>> AdvancedSearch(List<Guid> boards, AdvancedSearchCommand query)
        {
            var result = new List<SearchableCard>();

            List<BoardInfo> boardInfos = new List<BoardInfo>();
            foreach (var boardId in boards)
            {
                var boardInfo = await _boardService.GetBoardInfo(boardId);
                if (boardInfo != null)
                {
                    boardInfos.Add(boardInfo);
                }
            }

            var indexedBoards = await GetIndexedBoards(boardInfos.Select(bi => bi.IndexName).ToList());

            if (!indexedBoards.Any())
            {
                return Enumerable.Empty<SearchableCard>().ToList();
            }

            var indexQueries = new List<QueryMultiIndices>();

            foreach (var index in indexedBoards)
            {
                if (!string.IsNullOrEmpty(query.Title))
                {
                    indexQueries.Add(new QueryMultiIndices(index, query.Title)
                    {
                        RestrictSearchableAttributes = new List<string> { "title" }
                    });
                }

                if (!string.IsNullOrEmpty(query.Description))
                {
                    indexQueries.Add(new QueryMultiIndices(index, query.Description)
                    {
                        RestrictSearchableAttributes = new List<string> { "description" }
                    });
                }

                if (query.ListId.HasValue)
                {
                    indexQueries.Add(new QueryMultiIndices(index, query.ListId.ToString())
                    {
                        RestrictSearchableAttributes = new List<string> { "listId" }
                    });
                }

                if (query.HasAttachments)
                {
                    indexQueries.Add(new QueryMultiIndices(index)
                    {
                        RestrictSearchableAttributes = new List<string> { "hasAttachments" },
                        Filters = "hasAttachments:true"
                    });
                }

                if (query.DueDate.HasValue)
                {
                    var startDate = query.DueDate.Value.Date;
                    DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    long unixStartDateTime = (long)(startDate - sTime).TotalSeconds;

                    var endDate = startDate.AddDays(1);
                    long unixEndDateTime = (long)(endDate - sTime).TotalSeconds;

                    indexQueries.Add(new QueryMultiIndices(index, query.ListId.ToString())
                    {
                        RestrictSearchableAttributes = new List<string> { "dueDate" },
                        Filters = $"dueDate:{unixStartDateTime} TO {unixEndDateTime}"
                    });
                }
            }

            MultipleQueriesRequest request = new MultipleQueriesRequest
            {
                Requests = indexQueries
            };

            var multiQueryResult = await _searchClient.MultipleQueriesAsync<IndexedCard>(request);

            var hits = multiQueryResult.Results.SelectMany(x => x.Hits);

            foreach (var indexedCard in hits)
            {
                var searchableCard = new SearchableCard()
                {
                    CardId = indexedCard.ObjectID,
                    Title = indexedCard.Title,
                    IssueType = indexedCard.IssueType,
                    Status = indexedCard.Status,
                    SerialKey = indexedCard.SerialKey,
                    BoardId = indexedCard.BoardId
                };

                var board = boardInfos.FirstOrDefault(bi => bi.Id == indexedCard.BoardId);

                if (board != null)
                {
                    searchableCard.BoardTitle = board.Title;
                    searchableCard.BoardId = board.Id;

                    var list = board.Lists.FirstOrDefault(l => l.Id == indexedCard.ListId);

                    if (list != null)
                    {
                        searchableCard.List = list.Title;

                        searchableCard.IsComplete = list.CardStatus == Domain.Enums.BoardListCardStatus.DONE;
                    }
                }

                result.Add(searchableCard);
            }

            return result.DistinctBy(card => card.CardId).OrderBy(card => card.SerialKey).ToList();
        }

        private async Task<List<string>> GetIndexedBoards(List<string> userBoardIndices)
        {
            var boardIds = new List<string>();
            var indices = await _searchClient.ListIndicesAsync();

            if (indices == null)
            {
                return boardIds;
            }

            return indices.Items.Select(i => i.Name)
                .Where(userBoardIndices.Contains).ToList();
        }

        private void BuildQuery(StringBuilder queryBuilder, object term)
        {
            if (queryBuilder.Length == 0)
            {
                queryBuilder.Append(term);
            }
            else
            {
                queryBuilder.Append($" {term}");
            }
        }

        private void BuildFilter(StringBuilder filterBuilder, object filter)
        {
            if (filterBuilder.Length == 0)
            {
                filterBuilder.Append($"({filter})");
            }
            else
            {
                filterBuilder.Append($" AND ({filter})");
            }
        }
    }
}
