using Harmony.Application.Features.Cards.Commands.CreateChecklist;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class CreateCheckListModal
    {
        private readonly CreateCheckListCommand _createCheckListModel = new();
        private bool _processing;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public Guid CardId { get; set; }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task SubmitAsync()
        {
            _processing = true;

            var response = await _checkListManager
                .CreateCheckListAsync(new CreateCheckListCommand(CardId, _createCheckListModel.Title));

            MudDialog.Close(DialogResult.Ok(response.Data));

            DisplayMessage(response);

            _processing = false;
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
