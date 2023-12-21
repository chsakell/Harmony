using Harmony.Application.DTO.Search;
using Harmony.Application.Features.Search.Queries.GlobalSearch;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface ISearchManager : IManager
    {
        Task<IResult<List<SearchableCard>>> SearchCards(string text);
    }
}
