using Harmony.Application.DTO.Search;
using Harmony.Shared.Utilities;
using MediatR;
using MudBlazor;

namespace Harmony.Client.Shared.Components
{
    public partial class AppSearch
    {
        private SearchableCard _selectedCard { get; set; }
        private MudAutocomplete<SearchableCard> _autoComplete;
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

        private void Navigate(SearchableCard card)
        {
            _autoComplete.Clear();

            var slug = StringUtilities.SlugifyString(card.BoardTitle);

            _navigationManager.NavigateTo($"boards/{card.BoardId}/{slug}?/?cardId={card.CardId}");
        }
    }
}
