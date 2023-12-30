using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Commands.CreateBacklog;
using Harmony.Application.Features.Search.Commands.AdvancedSearch;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class AdvancedSearchModal
    {
        private bool _processing;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private MudDatePicker _datePicker;
        private List<IssueTypeDto> _issueTypes = new List<IssueTypeDto>();
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
            _processing = true;

            var result = await _searchManager.AdvancedSearch(_advancedSearchCommandModel);

            //MudDialog.Close(result.Data);

            //DisplayMessage(result);

            _processing = false;
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
