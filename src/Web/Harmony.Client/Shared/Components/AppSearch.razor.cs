using Harmony.Application.DTO;
using Harmony.Application.DTO.Search;
using Harmony.Client.Shared.Modals;
using Harmony.Domain.Enums;
using Harmony.Shared.Utilities;
using Harmony.Shared.Wrapper;
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
            else
            {
                DisplayMessage(searchResult);
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

        private async Task AdvancedSearch()
        {
            var parameters = new DialogParameters<AdvancedSearchModal>
            {
            };

            var options = new DialogOptions { CloseButton = true, FullScreen=true, MaxWidth = MaxWidth.Large, FullWidth = true, DisableBackdropClick = false };
            var dialog = _dialogService.Show<AdvancedSearchModal>("Advanced search", parameters, options);
            var result = await dialog.Result;
        }

        private async Task Navigate(SearchableCard card)
        {
            _autoComplete.Clear();

            var slug = StringUtilities.SlugifyString(card.BoardTitle);

            await JSRuntime.InvokeVoidAsync("resetAppSearchWidth");

            if(Enum.TryParse<CardStatus>(card.Status, out var status))
            {
                var boardKey = card.SerialKey.Split('-')[0];
                switch(status)
                {
                    case CardStatus.Backlog:
                        await EditCard(new CardDto() { Id = Guid.Parse(card.CardId) }, card.BoardId, boardKey);
                        break;
                    default:
                        await EditCard(new CardDto() { Id = Guid.Parse(card.CardId) }, card.BoardId, boardKey);
                        break;
                }
            }
        }

        private async Task EditCard(CardDto card, Guid boardId, string boardKey)
        {
            var parameters = new DialogParameters<EditCardModal>
            {
                { c => c.CardId, card.Id },
                { c => c.BoardId, boardId },
                { c => c.BoardKey, boardKey }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true, DisableBackdropClick = false };
            var dialog = _dialogService.Show<EditCardModal>(_localizer["Edit card"], parameters, options);
            var result = await dialog.Result;
        }

        private void DisplayMessage(IResult result)
        {
            if (result == null)
            {
                return;
            }

            var severity = result.Succeeded ? Severity.Success : Severity.Error;

            foreach (var message in result.Messages)
            {
                _snackBar.Add(message, severity);
            }
        }
    }
}
