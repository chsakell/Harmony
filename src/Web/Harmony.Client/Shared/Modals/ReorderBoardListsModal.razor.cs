using Harmony.Application.Features.Lists.Commands.UpdateListsPositions;
using Harmony.Client.Infrastructure.Models.Board;
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
        public IEnumerable<OrderedBoardListModel> Lists { get; set; }

        public IEnumerable<short> AvailablePositions => Lists.Select(l => l.Position);
        private void Cancel()
        {
            MudDialog.Cancel();
        }

     

        private async Task UpdateListOrders()
        {
            _processing = true;

            var listPositions = Lists
                .ToDictionary(keySelector: x => x.Id, elementSelector: x => x.Position);

            var result = await _boardManager.UpdateBoardListsPositions(
                new UpdateListsPositionsCommand(boardId: BoardId, listPositions));

            DisplayMessage(result);

            _processing = false;

            MudDialog.Close(result.Data);
        }

        private void SetPosition(Guid listId, short position)
        {
            var positionList = Lists.FirstOrDefault(l => l.Id == listId);
            var positionListCurrentPosition = positionList.Position;

            if(positionListCurrentPosition == position)
            {
                return;
            }

            var swapList = Lists.FirstOrDefault(l => l.Position == position);

            positionList.Position = position;
            swapList.Position = positionListCurrentPosition;
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
