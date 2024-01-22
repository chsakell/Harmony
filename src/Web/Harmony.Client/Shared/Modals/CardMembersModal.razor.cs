using Harmony.Application.Features.Cards.Commands.AddUserCard;
using Harmony.Application.Features.Cards.Commands.RemoveUserCard;
using Harmony.Application.Features.Cards.Queries.GetCardMembers;
using Harmony.Application.Features.Workspaces.Queries.SearchWorkspaceUsers;
using Harmony.Client.Shared.Dialogs;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class CardMembersModal
    {
        private bool _processing;
        private bool _processingMember;
        private bool _loading;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private List<CardMemberResponse> _boardMembers = new List<CardMemberResponse>();
        private SearchWorkspaceUserResponse _newBoardMember;
        private UserBoardAccess _newBoardMemberAccessLevel = UserBoardAccess.Member;

        private bool _searching;

        [Parameter]
        public Guid CardId { get; set; }

        [Parameter]
        public Guid BoardId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _loading = true;

            var boardMembersResult = await _cardManager.GetCardMembersAsync(CardId.ToString());

            if (boardMembersResult.Succeeded)
            {
                _boardMembers = boardMembersResult.Data;
            }

            _loading = false;
        }

        private async Task<IEnumerable<SearchWorkspaceUserResponse>> SearchUsers(string value)
        {
            if(string.IsNullOrEmpty(value) || value.Length < 4 || _searching)
            {
                return Enumerable.Empty<SearchWorkspaceUserResponse>();
            }

            _searching = true;
            var query = new SearchWorkspaceUsersQuery(_workspaceManager.SelectedWorkspace.Id, value);
            var searchResult = await _workspaceManager.SearchWorkspaceMembers(query);

            if(searchResult.Succeeded) {
                _searching = false;
                return searchResult.Data;
            }

            _searching = false;
            return Enumerable.Empty<SearchWorkspaceUserResponse>();
        }

        private async Task AddWorkspaceUserToCard()
        {
            _processing = true;

            var user = new CardMemberResponse()
            {
                Id = _newBoardMember.Id,
                FirstName = _newBoardMember.FirstName,
                LastName = _newBoardMember.LastName,
                UserName = _newBoardMember.UserName,
                ProfilePicture = _newBoardMember.ProfilePicture,
                Email = _newBoardMember.Email
            };

            await AddMember(user, false);

            _newBoardMember = null;
            _newBoardMemberAccessLevel = UserBoardAccess.Member;

            _processing = false;
        }
        
        private async Task RemoveMember(CardMemberResponse user)
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to remove {user.UserName} from the card?" },
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Error }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                _processingMember = true;

                var removeMemberResult = await _cardManager
                    .RemoveCardMemberAsync(new RemoveUserCardCommand(CardId, user.Id)
                    {
                        BoardId = BoardId
                    });

                if (removeMemberResult.Succeeded)
                {
                    user.IsMember = false;
                }

                DisplayMessage(removeMemberResult);

                _processingMember = false;
            }
        }

        private async Task AddMember(CardMemberResponse user, bool belongsToBoard = true)
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to add {user.UserName} to the card?" +
                (belongsToBoard ? string.Empty : " User will also be added to the board")},
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Success }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                _processingMember = true;

                var removeMemberResult = await _cardManager
                    .AddCardMemberAsync(new AddUserCardCommand(CardId, user.Id)
                    {
                        BoardId = BoardId
                    });

                if (removeMemberResult.Succeeded)
                {
                    var boardMember = _boardMembers.FirstOrDefault(m => m.Id == user.Id);

                    if(boardMember != null)
                    {
                        boardMember.IsMember = true;
                    }
                    else
                    {
                        user.IsMember = true;
                        _boardMembers.Add(user);
                    }
                }

                DisplayMessage(removeMemberResult);

                _processingMember = false;
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
