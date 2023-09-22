using Harmony.Application.Features.Workspaces.Queries.LoadWorkspace;
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

        private List<LoadWorkspaceResponse> _userBoards = new List<LoadWorkspaceResponse>();
        protected async override Task OnInitializedAsync()
        {
            var result = await _workspaceManager.LoadWorkspaceAsync(Id);

            if(result.Succeeded)
            {
                _userBoards = result.Data;
            }
        }

        private void NavigateToBoard(LoadWorkspaceResponse board)
        {
            var slug = StringUtilities.SlugifyString(board.Title.ToString());

            _navigationManager.NavigateTo($"boards/{board.Id}/{slug}");
        }
    }
}
