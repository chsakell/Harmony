using Harmony.Application.Features.Boards.Commands.AddUserBoard;
using Harmony.Application.Features.Boards.Commands.RemoveUserBoard;
using Harmony.Application.Features.Boards.Commands.UpdateUserBoardAccess;
using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Application.Features.Boards.Queries.SearchBoardUsers;
using Harmony.Client.Shared.Dialogs;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class BoardMembersModal
    {
        private bool _processing;
        private bool _removingMember;
        private bool _loading;

        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private List<UserBoardResponse> _boardMembers = new List<UserBoardResponse>();
        private SearchBoardUserResponse _newBoardMember;
        private UserBoardAccess _newBoardMemberAccessLevel = UserBoardAccess.Member;

        private bool _searching;

        [Parameter]
        public Guid BoardId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _loading = true;

            var boardMembersResult = await _boardManager.GetBoardMembersAsync(BoardId.ToString());

            if (boardMembersResult.Succeeded)
            {
                _boardMembers = boardMembersResult.Data;
            }

            _loading = false;
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

        private async Task ShareBoard()
        {
            _processing = true;

            var result = await _boardManager
                .AddBoardMemberAsync(new AddUserBoardCommand(BoardId, 
                _newBoardMember.Id, _newBoardMemberAccessLevel));

            if(result.Succeeded)
            {
                var member = result.Data;

                _boardMembers.Add(member);
            }

            _newBoardMember = null;
            _newBoardMemberAccessLevel = UserBoardAccess.Member;

            _processing = false;

            DisplayMessage(result);
        }

        private async Task UpdateBoardMemberAccess(UserBoardResponse user, UserBoardAccess access)
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want switch {user.UserName} to {access} level?" },
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Info }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                var updateResult = await _boardManager
                     .UpdateBoardUserAccessAsync(new UpdateUserBoardAccessCommand(BoardId, user.Id, access));

                if(updateResult.Succeeded)
                {
                    user.Access = access;
                }

                DisplayMessage(updateResult);
            }
        }

        private async Task RemoveMember(UserBoardResponse user)
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to remove {user.UserName} from the the board?" },
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Error }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                _removingMember = true;

                var removeMemberResult = await _boardManager
                    .RemoveBoardMemberAsync(new RemoveUserBoardCommand(BoardId, user.Id));

                if (removeMemberResult.Succeeded)
                {
                    _boardMembers.Remove(user);
                }

                DisplayMessage(removeMemberResult);

                _removingMember = false;
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
