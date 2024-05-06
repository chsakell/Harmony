using Harmony.Application.DTO;
using Harmony.Application.DTO.Search;
using Harmony.Application.SourceControl.DTO;
using Harmony.Client.Shared.Modals;
using Harmony.Domain.Enums;
using Harmony.Domain.SourceControl;
using Harmony.Shared.Utilities;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Slugify;

namespace Harmony.Client.Shared.Components
{
    public partial class CardRepositoryActivity
    {
        [Parameter] public string SerialKey { get; set; }
        [Parameter] public string CardTitle { get; set; }
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

        private string GetGitSlug()
        {
            return $"git checkout -b {SerialKey.ToLower()}-{StringUtilities.SlugifyString(CardTitle)}";
        }

        private async Task CopyToClipboard()
        {
            await _jsRuntime.InvokeVoidAsync("clipboardCopy.copyText", GetGitSlug());
            DisplayMessage(Result<bool>.Success(true, $"Copied to clipboard!"));
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
            counter += _branches.SelectMany(b => b.Tags).Count();

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

        public void AddCommitsToBranch(string branchName, RepositoryUserDto author, List<CommitDto> commits)
        {
            var branch = _branches.FirstOrDefault(b => b.Name == branchName);

            if (branch == null)
            {
                return;
            }

            foreach (var commit in commits)
            {
                if (!branch.Commits.Any(c => c.Id == commit.Id))
                {
                    branch.Commits.Add(commit);
                }
            }

            DisplayMessage(Result<bool>.Success(true, $"{author?.Login} pushed {commits.Count} commits to {branchName}"));

            StateHasChanged();
        }

        public void AddTagToBranch(string branchName, string tag)
        {
            var branch = _branches.FirstOrDefault(b => b.Name == branchName);

            if (branch == null)
            {
                return;
            }

            branch.Tags.Add(tag);

            DisplayMessage(Result<bool>.Success(true, $"{tag} created for {branchName}"));

            StateHasChanged();
        }

        public void AddPullRequestToBranch(PullRequestDto pullRequest, RepositoryUserDto sender)
        {
            if (pullRequest == null)
            {
                return;
            }

            var branch = _branches.FirstOrDefault(b => b.Name == pullRequest.SourceBranch);

            if (branch == null)
            {
                return;
            }

            if (!branch.PullRequests.Any(p => p.Id == pullRequest.Id))
            {
                branch.PullRequests.Add(pullRequest);
            }

            DisplayMessage(Result<bool>.Success(true, $"{sender?.Login} {pullRequest.State} {pullRequest.SourceBranch} pull request"));

            StateHasChanged();
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
