using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Commands.CreateSprint;
using Harmony.Application.Features.Cards.Commands.CreateBacklog;
using Harmony.Application.Features.Lists.Commands.CreateList;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;

namespace Harmony.Client.Shared.Modals
{
    public partial class CreateSprintModal
    {
        private bool _processing;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public CreateSprintCommand CreateSprintCommandModel { get; set; }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task SubmitAsync()
        {
            _processing = true;

            var result = await _boardManager.CreateSprintAsync(CreateSprintCommandModel);

            MudDialog.Close(result.Data);

            DisplayMessage(result);

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
