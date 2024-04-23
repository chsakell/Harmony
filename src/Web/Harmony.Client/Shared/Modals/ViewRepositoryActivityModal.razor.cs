using Harmony.Application.Features.Cards.Commands.UpdateCardDates;
using Harmony.Application.SourceControl.DTO;
using Harmony.Client.Infrastructure.Models.Labels;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class ViewRepositoryActivityModal
    {
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }


        [Parameter]
        public List<BranchDto> Branches { get; set; }

        private void Cancel()
        {
            MudDialog.Cancel();
        }
        void DisplayMessage(IResult result)
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
