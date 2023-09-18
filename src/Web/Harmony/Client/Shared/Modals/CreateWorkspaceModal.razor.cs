using Harmony.Application.Requests.Identity;
using Harmony.Application.Requests.Workspace;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class CreateWorkspaceModal
    {
        private readonly CreateWorkspaceRequest _createWorkspaceModel = new();

        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task SubmitAsync()
        {
            MudDialog.Close();

            //var response = await _userManager.RegisterUserAsync(_registerUserModel);
            //if (response.Succeeded)
            //{
            //    _snackBar.Add(response.Messages[0], Severity.Success);
            //    MudDialog.Close();
            //}
            //else
            //{
            //    foreach (var message in response.Messages)
            //    {
            //        _snackBar.Add(message, Severity.Error);
            //    }
            //}
        }
    }
}
