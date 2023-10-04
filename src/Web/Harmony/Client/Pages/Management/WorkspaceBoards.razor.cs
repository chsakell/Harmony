using Harmony.Application.DTO;
using Harmony.Application.Events;
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

            _boardManager.OnBoardCreated += BoardManager_OnBoardCreated;

            _loading = false;
        }

        private void BoardManager_OnBoardCreated(object? sender, BoardCreatedEvent e)
        {
            if(e.WorkspaceId.Equals(Id))
            {
                _boards.Add(new GetWorkspaceBoardResponse()
                {
                    Description = e.Description,
                    Title = e.Title,
                    Id = e.BoardId,
                    Lists = new List<BoardListDto>(),
                    Visibility = e.Visibility
                });

                StateHasChanged();
            }
        }
    }
}
