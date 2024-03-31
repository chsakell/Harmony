using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Commands.CreateSprintIssue;
using Harmony.Application.Features.Lists.Queries.GetBoardLists;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class CreateSprintIssueModal
    {
        private bool _processing;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public CreateSprintIssueCommand CreateSprintIssueCommandModel { get; set; }

        private List<IssueTypeDto> _issueTypes = new List<IssueTypeDto>();

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private GetBoardListResponse _selectedBoardList;
        private List<GetBoardListResponse> _boardLists = new List<GetBoardListResponse>();


        protected override async Task OnInitializedAsync()
        {
            var issueTypesResult = await _boardManager.GetIssueTypesAsync(CreateSprintIssueCommandModel.BoardId.ToString());

            if(issueTypesResult.Succeeded)
            {
                _issueTypes = issueTypesResult.Data;
            }

            var boardListsResult = await _boardManager
                .GetBoardListsAsync(CreateSprintIssueCommandModel.BoardId.ToString());

            if ((boardListsResult.Succeeded))
            {
                _boardLists = boardListsResult.Data.OrderBy(l => l.Position).ToList();

                if (_boardLists.Any())
                {
                    _selectedBoardList = _boardLists.First();
                    CreateSprintIssueCommandModel.BoardListId = _selectedBoardList.Id;
                }
            }
        }

        private void SelectedBoardList(GetBoardListResponse boardList)
        {
            _selectedBoardList = boardList;

            CreateSprintIssueCommandModel.BoardListId = _selectedBoardList.Id;
        }

        private async Task SubmitAsync()
        {
            _processing = true;

            var result = await _sprintManager.CreateSprintCardAsync(CreateSprintIssueCommandModel);

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
