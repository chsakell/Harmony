using Harmony.Application.DTO;
using Harmony.Application.DTO.Search;
using Harmony.Application.SourceControl.DTO;
using Harmony.Client.Shared.Modals;
using Harmony.Domain.Enums;
using Harmony.Shared.Utilities;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Harmony.Client.Shared.Components
{
    public partial class CardRepositoryActivity
    {
        [Parameter] public string SerialKey { get; set; }

        private List<BranchDto> _branches = new List<BranchDto>();
        private bool _loading;
        private bool _dataLoaded;

        protected override async Task OnInitializedAsync()
        {
            _loading = true;
            var cardBranchesResult = await _repositoryManager.GetCardBranches(SerialKey);

            if (cardBranchesResult.Succeeded)
            {
                _branches = cardBranchesResult.Data;
            }

            _dataLoaded = true;
        }

        private async Task ViewRepositoryActivityDetails()
        {
            var parameters = new DialogParameters<ViewRepositoryActivityModal>
            {
                {
                    modal => modal.Branches, _branches
                },
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<ViewRepositoryActivityModal>("View repository activity", parameters, options);
            var result = await dialog.Result;
        }

        private bool ShowArrows()
        {
            if (!_branches.Any())
            {
                return false;
            }

            var counter = 0;

            counter += _branches.Count;
            counter += _branches.SelectMany(b => b.PullRequests).Count();

            return counter > 1;
        }

        public void AddBranch(BranchDto branch)
        {
            if (!_branches.Any(b => b.Id == branch.Id))
            {
                _branches.Add(branch);

                DisplayMessage(Result<bool>.Success(true, $"Branch {branch.Name} created by {branch.Creator.Login}"));
                StateHasChanged();
            }
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
    }
}
