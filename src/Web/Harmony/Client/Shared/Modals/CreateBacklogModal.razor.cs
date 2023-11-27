using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Commands.CreateBacklog;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class CreateBacklogModal
    {
        private bool _processing;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public CreateBacklogCommand CreateBacklogCommandModel { get; set; }

        private List<IssueTypeDto> _issueTypes = new List<IssueTypeDto>();

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        protected override async Task OnInitializedAsync()
        {
            var issueTypesResult = await _boardManager.GetIssueTypesAsync(CreateBacklogCommandModel.BoardId.ToString());

            if(issueTypesResult.Succeeded)
            {
                _issueTypes = issueTypesResult.Data;
            }
        }

        private async Task SubmitAsync()
        {
            _processing = true;

            var result = await _cardManager.CreateBacklogItemAsync(CreateBacklogCommandModel);

            MudDialog.Close(result.Data);

            DisplayMessage(result);

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
