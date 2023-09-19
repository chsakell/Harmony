using Harmony.Client.Shared.Modals;
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
            if (!result.Cancelled)
            {
                // TODO update workspace list or navigate to it
            }
        }

        private async Task OpenCreateBoardModal()
        {
            var parameters = new DialogParameters();
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<CreateBoardModal>(_localizer["Create Board"], parameters, options);
            var result = await dialog.Result;
            if (!result.Cancelled)
            {
                // TODO update workspace list or navigate to it
            }
        }
    }
}
