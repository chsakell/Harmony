using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Application.Features.Boards.Queries.SearchBoardUsers;
using Harmony.Application.Features.Workspaces.Commands.AddMember;
using Harmony.Application.Features.Workspaces.Commands.RemoveMember;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
using Harmony.Client.Shared.Dialogs;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class BoardMembersModal
    {
        private bool _processing;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private List<UserBoardResponse> _boardMembers = new List<UserBoardResponse>();
        private SearchBoardUserResponse _newBoardMember;
        private bool _searching;

        [Parameter]
        public Guid BoardId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var boardMembersResult = await _boardManager.GetBoardMembersAsync(BoardId.ToString());

            if (boardMembersResult.Succeeded)
            {
                _boardMembers = boardMembersResult.Data;
            }
        }

        private async Task<IEnumerable<SearchBoardUserResponse>> SearchUsers(string value)
        {
            if(string.IsNullOrEmpty(value) || value.Length < 4 || _searching)
            {
                return Enumerable.Empty<SearchBoardUserResponse>();
            }

            _searching = true;
            var searchResult = await _boardManager.SearchBoardMembersAsync(BoardId.ToString(), value);

            if(searchResult.Succeeded) {
                _searching = false;
                return searchResult.Data;
            }

            _searching = false;
            return Enumerable.Empty<SearchBoardUserResponse>();
        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }
        
        private async Task AddMember(UserWorkspaceResponse user)
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to add {user.UserName} as a member to the workspace?" },
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Success }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                var addMemberResult = await _workspaceManager.AddWorkspaceMember(new AddWorkspaceMemberCommand()
                {
                    UserId = user.Id,
                    WorkspaceId = BoardId
                });

                if (addMemberResult.Succeeded)
                {
                    user.IsMember = true;
                }

                DisplayMessage(addMemberResult);
            }

        }

        private async Task RemoveMember(UserWorkspaceResponse user)
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to remove {user.UserName} from the workspace?" },
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Error }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                var removeMemberResult = await _workspaceManager.RemoveWorkspaceMember(new RemoveWorkspaceMemberCommand()
                {
                    UserId = user.Id,
                    WorkspaceId = BoardId
                });

                if (removeMemberResult.Succeeded)
                {
                    user.IsMember = false;
                }

                DisplayMessage(removeMemberResult);
            }
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
