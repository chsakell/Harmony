using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Commands.CreateBacklog;
using Harmony.Application.Features.Lists.Queries.GetBoardLists;
using Harmony.Application.Features.Retrospectives.Commands.Create;
using Harmony.Domain.Enums;
using Harmony.Domain.Enums.Automations;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class CreateRetrospectiveModal
    {
        private bool _processing;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public CreateRetrospectiveCommand CreateRetrospectiveCommandModel { get; set; }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task SubmitAsync()
        {
            _processing = true;

            var result = await _retrospectiveManager.Create(CreateRetrospectiveCommandModel);

            MudDialog.Close(result.Data);

            DisplayMessage(result);

            _processing = false;
        }

        Func<RetrospectiveType, string> typeConverter = p =>
        {
            return p switch
            {
                RetrospectiveType.WentWell_ToImprove_ActionItems => "Went Well - To Improve - Action Items",
                RetrospectiveType.Start_Stop_Continue => "Start - Stop - Continue",
                _ => "Select Type"
            };
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
