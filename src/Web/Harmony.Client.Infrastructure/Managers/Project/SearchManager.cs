using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using Harmony.Application.DTO.Search;
using Harmony.Application.Features.Search.Queries.GlobalSearch;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    /// <summary>
    /// Client manager for searching
    /// </summary>
    public class SearchManager : ISearchManager
    {
        private readonly HttpClient _httpClient;


        public SearchManager(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<IResult<List<SearchableCard>>> SearchCards(string text)
        {
            var response = await _httpClient.GetAsync(Routes.SearchEndpoints.Search(text));

            return await response.ToResult<List<SearchableCard>>();
        }
    }
}
