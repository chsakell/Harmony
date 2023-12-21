using Harmony.Application.DTO.Search;
using Harmony.Application.Features.Search.Queries.GlobalSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Services.Search
{
    public interface ISearchService
    {
        bool CreateIndex(string name);
        Task AddCardToIndex(Guid boardId, SearchableCard card);
        Task UpdateCard(Guid boardId, SearchableCard card);
        Task<List<SearchableCard>> SearchBoard(Guid boardId, string term);
    }
}
