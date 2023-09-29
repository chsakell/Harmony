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

        private void NavigateToBoard(LoadWorkspaceResponse board)
        {
            var slug = StringUtilities.SlugifyString(board.Title.ToString());

            _navigationManager.NavigateTo($"boards/{board.Id}/{slug}");
        }

        protected async override Task OnParametersSetAsync()
        {
            var result = await _workspaceManager.LoadWorkspaceAsync(Id);

            if (result.Succeeded)
            {
                await _workspaceManager.SelectWorkspace(Guid.Parse(Id));
                _userBoards = result.Data;
            }
        }
    }
}
