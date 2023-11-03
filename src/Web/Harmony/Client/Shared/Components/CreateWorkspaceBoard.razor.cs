using Harmony.Client.Shared.Modals;
using MudBlazor;

namespace Harmony.Client.Shared.Components
{
    public partial class CreateWorkspaceBoard
    {
        
        private void OpenCreateWorkspaceModal()
        {
            var parameters = new DialogParameters();
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            _dialogService.Show<CreateWorkspaceModal>(_localizer["Create Workspace"], parameters, options);
        }

        private void OpenCreateBoardModal()
        {
            var parameters = new DialogParameters();
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            _dialogService.Show<CreateBoardModal>(_localizer["Create Board"], parameters, options);
        }
    }
}
