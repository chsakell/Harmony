using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Domain.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class CreateBoardModal
    {
        private readonly CreateBoardCommand _createBoardModel = new();
        private bool _processing;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }
        List<WorkspaceDto> _ownedWorkspaces = new List<WorkspaceDto>();
        WorkspaceDto? _selectedWorkspace;

        [Parameter]
        public Guid? FilterWorkspaceId { get; set; }

        [Parameter]
        public BoardType? Type { get; set; }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        protected async override Task OnInitializedAsync()
        {
            var ownedWorkspacesResult = await _workspaceManager.GetAllAsync();

            if (ownedWorkspacesResult.Succeeded)
            {
                _ownedWorkspaces = ownedWorkspacesResult.Data;
            }

            if(Type.HasValue)
            {
                _createBoardModel.BoardType = Type.Value;
            }
        }

        private async Task SubmitAsync()
        {
            _processing = true;
            var response = await _boardManager.CreateAsync(_createBoardModel);

            if (response.Succeeded)
            {
                _snackBar.Add(response.Messages[0], Severity.Success);
                MudDialog.Close();
            }
            else
            {
                foreach (var message in response.Messages)
                {
                    _snackBar.Add(message, Severity.Error);
                }
            }

            _processing = false;
        }

        private void SetTitle(string title)
        {
            _createBoardModel.Title = title.Trim();

            if(!string.IsNullOrEmpty(_createBoardModel.Title) && _createBoardModel.Title.Length > 3)
            {
                _createBoardModel.Key = title.Substring(0, 4).ToUpper();
            }
        }

        Func<WorkspaceDto, string> converter = p => p?.Name;
    }
}
