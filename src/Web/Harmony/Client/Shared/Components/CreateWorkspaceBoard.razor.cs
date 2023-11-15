using Harmony.Application.DTO;
using Harmony.Client.Shared.Modals;
using Harmony.Domain.Enums;
using Harmony.Shared.Utilities;
using MudBlazor;

namespace Harmony.Client.Shared.Components
{
    public partial class CreateWorkspaceBoard
    {
        
        private async Task OpenCreateWorkspaceModal()
        {
            var parameters = new DialogParameters();
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<CreateWorkspaceModal>(_localizer["Create Workspace"], parameters, options);

            var result = await dialog.Result;
            if (!result.Canceled)
            {
                var workspace = result.Data as WorkspaceDto;
                if (workspace != null)
                {
                    Navigate(workspace);
                }
            }
        }

        private void OpenCreateBoardModal(BoardType type)
        {
            var parameters = new DialogParameters<CreateBoardModal>()
            {
                {
                    modal => modal.Type, type
                }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            _dialogService.Show<CreateBoardModal>(_localizer["Create Board"], parameters, options);
        }

        private void Navigate(WorkspaceDto workspace)
        {
            if (string.IsNullOrEmpty(workspace?.Name))
            {
                return;
            }

            var slug = StringUtilities.SlugifyString(workspace.Name);
            _navigationManager.NavigateTo($"workspaces/{workspace.Id}/{slug}");
        }
    }
}
