using Harmony.Application.Features.Boards.Queries.GetAllForUser;
using Harmony.Shared.Utilities;
using Microsoft.AspNetCore.Components;

namespace Harmony.Client.Pages.Management
{
    public partial class Workspace
    {
        [Parameter]
        public string Id { get; set; }

        [Parameter]
        public string Name { get; set; }

        private List<GetAllForUserBoardResponse> _userBoards = new List<GetAllForUserBoardResponse>();
        protected async override Task OnInitializedAsync()
        {
            var result = await _boardManager.GetUserBoardsAsync();

            if(result.Succeeded)
            {
                _userBoards = result.Data;
            }
        }

        private void NavigateToBoard(GetAllForUserBoardResponse board)
        {
            var slug = StringUtilities.SlugifyString(board.Title.ToString());

            _navigationManager.NavigateTo($"boards/{board.Id}/{slug}");
        }
    }
}
