using Harmony.Application.Features.Boards.Queries.GetArchivedItems;
using Harmony.Application.Features.Cards.Commands.MoveToSprint;
using Harmony.Application.Features.Cards.Commands.ReactivateCards;
using Harmony.Application.Features.Lists.Queries.GetBoardLists;
using Harmony.Domain.Entities;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static MudBlazor.CategoryTypes;

namespace Harmony.Client.Shared.Modals
{
    public partial class ReactivateCardsModal
    {
        private bool _processing;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public Guid BoardId { get; set; }

        [Parameter]
        public HashSet<GetArchivedItemResponse> Items { get; set; }
        private List<GetBoardListResponse> _boardLists = new List<GetBoardListResponse>();
        private GetBoardListResponse _selectedBoardList;

        protected override async Task OnInitializedAsync()
        {
            var boardListsResult = await _boardManager
                .GetBoardListsAsync(BoardId.ToString());

            if ((boardListsResult.Succeeded))
            {
                _boardLists = boardListsResult.Data.OrderBy(l => l.Position).ToList();

                if (_boardLists.Any())
                {
                    _selectedBoardList = _boardLists.First();
                }
            }
        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task Reactivate()
        {
            _processing = true;

            var result = await _boardManager
                .ReactivateCards(new ReactivateCardsCommand(BoardId,
                _selectedBoardList.Id, Items.Select(i => i.Id).ToList()));

            DisplayMessage(result);

            _processing = false;

            if (result.Succeeded)
            {
                MudDialog.Close(result.Data);
            }
        }

        Func<GetBoardListResponse, string> converter = p =>
        {
            return p?.Title ?? "Move to list";
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
