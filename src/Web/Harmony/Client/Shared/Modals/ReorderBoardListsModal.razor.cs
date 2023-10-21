using Harmony.Application.DTO;
using Harmony.Application.Features.Lists.Commands.CreateList;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class ReorderBoardListsModal
    {
        private bool _processing;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }


        [Parameter]
        public Guid BoardId { get; set; }

        [Parameter]
        public Guid CardId { get; set; }

        [Parameter]
        public IEnumerable<BoardListDto> Lists { get; set; }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

     

        private async Task SubmitAsync()
        {
            //_processing = true;

            //var result = await _boardListManager.CreateListAsync(CreateListCommandModel);

            //MudDialog.Close(result.Data);

            //DisplayMessage(result);

            //_processing = false;
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
