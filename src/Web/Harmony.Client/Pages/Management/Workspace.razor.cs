using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Features.Workspaces.Commands.UpdateStatus;
using Harmony.Client.Shared.Dialogs;
using Harmony.Client.Shared.Modals;
using Harmony.Shared.Utilities;
using Harmony.Shared.Wrapper;
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

        private async Task Archive()
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to archive this workspace?<br>" +
                $"You won't be able to view its boards unless it's activated again."},
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Error }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Canceled)
            {
                var command = new UpdateWorkspaceStatusCommand()
                {
                    Id = Guid.Parse(Id),
                    Status = Domain.Enums.WorkspaceStatus.Archived
                };

                var result = await _workspaceManager.UpdateWorkspaceStatus(command);

                DisplayMessage(result);

                if (result.Succeeded)
                {
                    await _clientPreferenceManager.ClearSelectedWorkspace(Guid.Parse(Id));
                    _navigationManager.NavigateTo("/", forceLoad: true);
                }
            }
        }

        private async Task Edit()
        {
            var parameters = new DialogParameters<CreateWorkspaceModal>
                {
                    { c => c.WorkspaceId, Guid.Parse(Id) },
                };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, BackdropClick = false };
            var dialog = _dialogService.Show<CreateWorkspaceModal>(_localizer["Edit Workspace"], parameters, options);

            var result = await dialog.Result;
            if (!result.Canceled)
            {
                var workspace = result.Data as WorkspaceDto;
                var slug = StringUtilities.SlugifyString(workspace.Name);
                _navigationManager.NavigateTo($"workspaces/{workspace.Id}/{slug}");
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

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, BackdropClick = false };
            var dialog = _dialogService.Show<CreateBoardModal>(_localizer["Create Board"], parameters, options);
            var result = await dialog.Result;
            if (!result.Canceled)
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

        public void Dispose()
        {
            _boardManager.OnBoardCreated -= BoardManager_OnBoardCreated;
            _userBoardsLoading = true;
        }
    }
}
