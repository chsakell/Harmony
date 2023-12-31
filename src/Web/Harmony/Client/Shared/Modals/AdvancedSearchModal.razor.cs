using Harmony.Application.DTO;
using Harmony.Application.DTO.Search;
using Harmony.Application.Features.Cards.Commands.CreateBacklog;
using Harmony.Application.Features.Search.Commands.AdvancedSearch;
using Harmony.Domain.Entities;
using Harmony.Shared.Utilities;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using static MudBlazor.CategoryTypes;

namespace Harmony.Client.Shared.Modals
{
    public partial class AdvancedSearchModal
    {
        private bool _searching;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private MudDatePicker _datePicker;
        private List<IssueTypeDto> _issueTypes = new List<IssueTypeDto>();
        private IEnumerable<SearchableCard> _cards = new List<SearchableCard>();
        private AdvancedSearchCommand _advancedSearchCommandModel = new AdvancedSearchCommand();
        private void Cancel()
        {
            MudDialog.Cancel();
        }

        protected override async Task OnInitializedAsync()
        {
            var initAdvancedSearchResult = await _searchManager.InitAdvancedSearch();

            if(initAdvancedSearchResult.Succeeded)
            {

            }
        }

        private async Task Search()
        {
            _searching = true;

            var result = await _searchManager.AdvancedSearch(_advancedSearchCommandModel);

            if(result.Succeeded)
            {
                _cards = result.Data;
            }

            _searching = false;
        }

        private void RowClickEvent(TableRowClickEventArgs<SearchableCard> tableRowClickEventArgs)
        {
            var card = tableRowClickEventArgs.Item;
            var slug = StringUtilities.SlugifyString(card.BoardTitle);

            MudDialog.Close();
            _navigationManager.NavigateTo($"boards/{card.BoardId}/{slug}/{card.CardId}");
        }

        private Func<IssueTypeDto, string> convertFunc = type =>
        {
            if(type.Id == Guid.Empty)
            {
                return "Select issue type";
            }

            return type.Summary;
        };

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
