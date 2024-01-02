using Harmony.Application.DTO.Search;
using Harmony.Application.Features.Search.Commands.AdvancedSearch;

namespace Harmony.Application.Contracts.Services.Search
{
    public interface ISearchService
    {
        Task<List<SearchableCard>> Search(List<Guid> boards, string term);
        Task<List<SearchableCard>> Search(List<Guid> boards, AdvancedSearchCommand query);
    }
}
