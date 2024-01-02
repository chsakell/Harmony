using Harmony.Application.DTO.Search;
using Harmony.Application.Features.Search.Commands.AdvancedSearch;
using Harmony.Application.Features.Search.Queries.InitAdvancedSearch;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface ISearchManager : IManager
    {
        Task<IResult<List<SearchableCard>>> SearchCards(string text);
        Task<IResult<InitAdvancedSearchResponse>> InitAdvancedSearch();
        Task<IResult<List<SearchableCard>>> AdvancedSearch(AdvancedSearchCommand command);
    }
}
