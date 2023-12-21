using Harmony.Application.DTO.Search;

namespace Harmony.Client.Shared.Components
{
    public partial class AppSearch
    {
        private SearchableCard _selectedCard { get; set; }

        private async Task<IEnumerable<SearchableCard>> Search(string value, CancellationToken token)
        {
            if(string.IsNullOrEmpty(value))
            {
                return new List<SearchableCard>() { };
            }
            var searchResult = await _searchManager.SearchCards(value);

            if(searchResult.Succeeded)
            {
                return searchResult.Data;
            }

            return Enumerable.Empty<SearchableCard>();
        }


    }
}
