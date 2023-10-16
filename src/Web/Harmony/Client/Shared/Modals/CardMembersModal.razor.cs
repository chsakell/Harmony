using Harmony.Application.Features.Boards.Commands.AddUserBoard;
using Harmony.Application.Features.Boards.Commands.RemoveUserBoard;
using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Application.Features.Boards.Queries.SearchBoardUsers;
using Harmony.Application.Features.Cards.Commands.AddUserCard;
using Harmony.Application.Features.Cards.Commands.RemoveUserCard;
using Harmony.Application.Features.Cards.Queries.GetCardMembers;
using Harmony.Application.Features.Workspaces.Commands.AddMember;
using Harmony.Application.Features.Workspaces.Commands.RemoveMember;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
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

        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private List<CardMemberResponse> _boardMembers = new List<CardMemberResponse>();
        private SearchBoardUserResponse _newBoardMember;
        private UserBoardAccess _newBoardMemberAccessLevel = UserBoardAccess.Member;

        private bool _searching;

        [Parameter]
        public Guid CardId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var boardMembersResult = await _cardManager.GetCardMembersAsync(CardId.ToString());

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
            var searchResult = await _boardManager.SearchBoardMembersAsync(CardId.ToString(), value);

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
                .AddBoardMemberAsync(new AddUserBoardCommand(CardId, 
                _newBoardMember.Id, _newBoardMemberAccessLevel));

            if(result.Succeeded)
            {
                var member = result.Data;

                //_boardMembers.Add(member);
            }

            _newBoardMember = null;
            _newBoardMemberAccessLevel = UserBoardAccess.Member;

            _processing = false;

            DisplayMessage(result);
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
                    .RemoveCardMemberAsync(new RemoveUserCardCommand(CardId, user.Id));

                if (removeMemberResult.Succeeded)
                {
                    user.IsMember = false;
                }

                DisplayMessage(removeMemberResult);

                _processingMember = false;
            }
        }

        private async Task AddMember(CardMemberResponse user)
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to add {user.UserName} to the card?" },
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Success }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                _processingMember = true;

                var removeMemberResult = await _cardManager
                    .AddCardMemberAsync(new AddUserCardCommand(CardId, user.Id));

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
