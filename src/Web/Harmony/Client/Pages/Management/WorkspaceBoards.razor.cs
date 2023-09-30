using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceBoards;
using Microsoft.AspNetCore.Components;

namespace Harmony.Client.Pages.Management
{
    public partial class WorkspaceBoards
    {
        [Parameter]
        public string Id { get; set; }

        [Parameter]
        public string Name { get; set; }

        private List<GetWorkspaceBoardResponse> _boards;
        private bool _loading;

        protected override async Task OnInitializedAsync()
        {
            _loading = true;

            var workspaceBoardsResult = await _workspaceManager.GetWorkspaceBoards(Id);

            if(workspaceBoardsResult.Succeeded)
            {
                _boards = workspaceBoardsResult.Data;
            }

            _loading = false;
        }
    }
}
