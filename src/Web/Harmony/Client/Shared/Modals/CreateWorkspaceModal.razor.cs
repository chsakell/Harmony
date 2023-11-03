using Harmony.Application.Features.Workspaces.Commands.Create;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class CreateWorkspaceModal
    {
        private readonly CreateWorkspaceCommand _createWorkspaceModel = new();
        private bool _processing;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task SubmitAsync()
        {
            _processing = true;
            var result = await _workspaceManager.CreateAsync(_createWorkspaceModel);

            if (result.Succeeded)
            {
                _snackBar.Add(result.Messages[0], Severity.Success);

                MudDialog.Close(result.Data);
            }
            else
            {
                foreach (var message in result.Messages)
                {
                    _snackBar.Add(message, Severity.Error);
                }
            }

            _processing = false;
        }
    }
}
