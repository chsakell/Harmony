using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceBoards;
using Harmony.Application.Features.Workspaces.Queries.LoadWorkspace;
using Harmony.Domain.Entities;
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

        protected override void OnInitialized()
        {
            _boardManager.OnBoardCreated += BoardManager_OnBoardCreated;
        }

        private void BoardManager_OnBoardCreated(object? sender, BoardCreatedEvent e)
        {
            if (e.WorkspaceId.Equals(Id))
            {
                _userBoards.Add(new LoadWorkspaceResponse()
                {
                    Description = e.Description,
                    Title = e.Title,
                    Id = e.BoardId,
                    WorkspaceId = Guid.Parse(Id)
                });

                StateHasChanged();
            }
        }

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
