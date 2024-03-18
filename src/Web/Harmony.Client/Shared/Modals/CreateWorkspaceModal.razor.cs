using Harmony.Application.Features.Workspaces.Commands.Create;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class CreateWorkspaceModal
    {
        private readonly CreateOrEditWorkspaceCommand _createWorkspaceModel = new();
        private bool _processing;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter] public Guid? WorkspaceId { get; set; }

        public bool IsEdit => WorkspaceId.HasValue;

        protected override async Task OnInitializedAsync()
        {
            if(WorkspaceId.HasValue)
            {
                var workspaceResult = await _workspaceManager.GetWorkspaceInfoAsync(WorkspaceId.Value);

                if(workspaceResult.Succeeded)
                {
                    var workspace = workspaceResult.Data;

                    _createWorkspaceModel.WorkspaceId = WorkspaceId;
                    _createWorkspaceModel.Name = workspace.Name;
                    _createWorkspaceModel.Description = workspace.Description;
                    _createWorkspaceModel.IsPublic = workspace.IsPublic;
                }
            }
        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task SubmitAsync()
        {
            _processing = true;
            var result = await _workspaceManager.CreateOrEdit(_createWorkspaceModel);

            if (result.Succeeded)
            {
                _snackBar.Add(result.Messages[0], Severity.Success);

                if(IsEdit)
                {
                    var workspaceUpdated = _workspaceManager.UserWorkspaces
                        .FirstOrDefault(w => w.Id == _createWorkspaceModel.WorkspaceId);

                    if(workspaceUpdated != null )
                    {
                        workspaceUpdated.Name = _createWorkspaceModel.Name;
                        StateHasChanged();
                    }
                }

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
