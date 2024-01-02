using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using Harmony.Application.DTO.Search;
using Harmony.Application.Features.Search.Queries.InitAdvancedSearch;
using Harmony.Application.Features.Search.Commands.AdvancedSearch;
using System.Net.Http.Json;

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

        public async Task<IResult<InitAdvancedSearchResponse>> InitAdvancedSearch()
        {
            var response = await _httpClient.GetAsync(Routes.SearchEndpoints.AdvancedSearch);

            return await response.ToResult<InitAdvancedSearchResponse>();
        }

        public async Task<IResult<List<SearchableCard>>> AdvancedSearch(AdvancedSearchCommand command)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.SearchEndpoints.AdvancedSearch, command);

            return await response.ToResult<List<SearchableCard>>();
        }
    }
}
