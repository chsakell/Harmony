using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetAllForUser;
using Harmony.Shared.Utilities;
using Microsoft.AspNetCore.Components;

namespace Harmony.Client.Pages.Management
{
    public partial class Board
    {
        [Parameter]
        public string Id { get; set; }

        [Parameter]
        public string Name { get; set; }

        private GetBoardResponse _board = new GetBoardResponse();
        protected async override Task OnInitializedAsync()
        {
            var result = await _boardManager.GetBoardAsync(Id);

            if(result.Succeeded)
            {
                _board = result.Data;
            }
        }

        private void NavigateToBoard(GetAllForUserBoardResponse board)
        {
            var slug = StringUtilities.SlugifyString(board.Id.ToString());

            _navigationManager.NavigateTo($"boards/{board.Id}/{slug}");
        }
    }
}
