using Harmony.Application.Features.Cards.Commands.CreateChildIssue;
using Harmony.Application.Features.Lists.Queries.GetBoardLists;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class CreateChildIssueModal
    {
        private bool _processing;
        private List<GetBoardListResponse> _boardLists = new List<GetBoardListResponse>();
        private GetBoardListResponse _selectedBoardList;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public CreateChildIssueCommand CreateChildIssueCommandModel { get; set; }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        protected override async Task OnInitializedAsync()
        {
            var boardListsResult = await _boardManager
                .GetBoardListsAsync(CreateChildIssueCommandModel.BoardId.ToString());

            if ((boardListsResult.Succeeded))
            {
                _boardLists = boardListsResult.Data.OrderBy(l => l.Position).ToList();

                if (_boardLists.Any())
                {
                    _selectedBoardList = _boardLists.First();
                }
            }
        }

        private async Task SubmitAsync()
        {
            _processing = true;

            CreateChildIssueCommandModel.ListId = _selectedBoardList.Id;

            var result = await _cardManager
                .CreateChildIssueAsync(CreateChildIssueCommandModel);

            MudDialog.Close(result.Data);

            if (!result.Succeeded)
            {
                DisplayMessage(result);
            }

            _processing = false;
        }

        Func<GetBoardListResponse, string> converter = p =>
        {
            return p?.Title ?? "Status";
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
