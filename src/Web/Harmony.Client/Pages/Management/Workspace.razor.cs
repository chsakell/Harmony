using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Client.Shared.Modals;
using Harmony.Shared.Utilities;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Pages.Management
{
    public partial class Workspace : IDisposable
    {
        [Parameter]
        public string Id { get; set; }

        [Parameter]
        public string Name { get; set; }

        private List<BoardDto> _userBoards = new List<BoardDto>();
        private bool _userBoardsLoading = true;

        protected override void OnInitialized()
        {
            _boardManager.OnBoardCreated += BoardManager_OnBoardCreated;
        }

        private void BoardManager_OnBoardCreated(object? sender, BoardCreatedEvent e)
        {
            if (e.WorkspaceId.Equals(Id))
            {
                _userBoards.Add(e.Board);

                StateHasChanged();
            }
        }

        private async Task OpenCreateBoardModal()
        {
            var parameters = new DialogParameters<CreateBoardModal>
            {
                {
                    modal => modal.FilterWorkspaceId, Guid.Parse(Id)
                }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<CreateBoardModal>(_localizer["Create Board"], parameters, options);
            var result = await dialog.Result;
            if (!result.Cancelled)
            {
                // TODO update workspace list or navigate to it
            }
        }

        private void NavigateToBoard(BoardDto board)
        {
            var slug = StringUtilities.SlugifyString(board.Title.ToString());

            _navigationManager.NavigateTo($"boards/{board.Id}/{slug}");
        }

        private string GetBoardUrl(BoardDto board)
        {
            var slug = StringUtilities.SlugifyString(board.Title.ToString());

            return $"boards/{board.Id}/{slug}";
        }

        protected async override Task OnParametersSetAsync()
        {
            _userBoardsLoading = true;

            var result = await _workspaceManager.LoadWorkspaceAsync(Id);

            if (result.Succeeded)
            {
                await _workspaceManager.SelectWorkspace(Guid.Parse(Id));
                _userBoards = result.Data;
            }

            _userBoardsLoading = false;
        }

        public void Dispose()
        {
            _boardManager.OnBoardCreated -= BoardManager_OnBoardCreated;
            _userBoardsLoading = true;
        }
    }
}
