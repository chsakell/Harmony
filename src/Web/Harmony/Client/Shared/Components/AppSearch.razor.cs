using Harmony.Application.DTO.Search;
using Harmony.Shared.Utilities;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Harmony.Client.Shared.Components
{
    public partial class AppSearch
    {
        private SearchableCard _selectedCard { get; set; }
        private MudAutocomplete<SearchableCard> _autoComplete;
        [Inject] private IJSRuntime JSRuntime { get; set; }
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

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(firstRender)
            {
                await JSRuntime.InvokeVoidAsync("toggleAppSearchWidth");
            }
        }

        private async Task Navigate(SearchableCard card)
        {
            _autoComplete.Clear();

            var slug = StringUtilities.SlugifyString(card.BoardTitle);

            await JSRuntime.InvokeVoidAsync("resetAppSearchWidth");

            _navigationManager.NavigateTo($"boards/{card.BoardId}/{slug}/{card.CardId}");
        }
    }
}
