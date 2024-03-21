
using Harmony.Application.Features.Boards.Commands.CreateSprint;
using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Features.Boards.Queries.GetSprintsSummary;
using Harmony.Application.Features.Sprints.Commands.StartSprint;
using Harmony.Application.Features.Workspaces.Commands.AddMember;
using Harmony.Application.Features.Workspaces.Queries.GetSprints;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
using Harmony.Client.Shared.Dialogs;
using Harmony.Client.Shared.Modals;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Pages.Management
{
    public partial class Sprints
    {
        [Parameter]
        public string Id { get; set; }

        [Parameter]
        public string Name { get; set; }

        private List<SprintSummary> _sprints;
        private MudTable<SprintSummary> _table;
        private string _searchString = "";
        private int _totalItems;
        public SprintStatus? _status { get; set; } = null;

        private bool _loading;


        private async Task<TableData<SprintSummary>> ReloadData(TableState state)
        {
            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                state.Page = 0;
            }
            await LoadData(state.Page, state.PageSize, state);

            return new TableData<SprintSummary>
            {
                TotalItems = _totalItems,
                Items = _sprints
            };
        }

        private void OnSearch(string text)
        {
            _searchString = string.IsNullOrEmpty(text) ? null : text;
            _table.ReloadServerData();
        }

        private async Task LoadData(int pageNumber, int pageSize, TableState state)
        {
            string[] orderings = null;
            if (!string.IsNullOrEmpty(state.SortLabel))
            {
                orderings = state.SortDirection != SortDirection.None ? new[] { $"{state.SortLabel} {state.SortDirection}" } : new[] { $"{state.SortLabel}" };
            }

            var request = new GetSprintsSummaryQuery(Guid.Parse(Id))
            {
                BoardId = Guid.Parse(Id),
                PageSize = pageSize,
                PageNumber = pageNumber + 1,
                SearchTerm = _searchString.ToLower().Trim(),
                OrderBy = orderings,
                Status = _status
            };

            var response = await _boardManager.GetSprintsDetails(request);
            if (response.Succeeded)
            {
                _totalItems = response.TotalCount;
                _sprints = response.Data;
            }
            else
            {
                foreach (var message in response.Messages)
                {
                    _snackBar.Add(message, Severity.Error);
                }
            }
        }

        private async Task EditSprint(SprintSummary sprint)
        {
            var parameters = new DialogParameters<CreateEditSprintModal>
            {
                {
                    modal => modal.CreateEditSprintCommandModel,
                    new CreateEditSprintCommand(Guid.Parse(Id))
                    {
                        SprintId = sprint.Id,
                        Name = sprint.Name,
                        StartDate = sprint.StartDate,
                        EndDate = sprint.EndDate,
                        Goal = sprint.Goal
                    }
                }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<CreateEditSprintModal>(_localizer["Edit sprint"], parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await _table.ReloadServerData();
            }
        }

        private async Task CreateSprint()
        {
            var parameters = new DialogParameters<CreateEditSprintModal>
            {
                {
                    modal => modal.CreateEditSprintCommandModel,
                    new CreateEditSprintCommand(Guid.Parse(Id))
                }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<CreateEditSprintModal>(_localizer["Create sprint"], parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await _table.ReloadServerData();
            }
        }

        private async Task StartSprint(SprintSummary sprint)
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to start {sprint.Name}? " +
                $"All of it's cards will be available on the board" },
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Warning }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Canceled)
            {
                var request = new StartSprintCommand(Guid.Parse(Id), sprint.Id);

                var result = await _sprintManager.StartSprint(request);

                if (result.Succeeded)
                {
                    await _table.ReloadServerData();
                }

                DisplayMessage(result);
            }
        }

        private async Task CompleteSprint(SprintSummary sprint)
        {
            var pendingSprintResult = await _boardManager
                .GetPendingSprintCards(new GetPendingSprintCardsQuery(Guid.Parse(Id), sprint.Id));

            if (pendingSprintResult.Succeeded)
            {
                var pendingCards = pendingSprintResult.Data.PendingCards;
                var availableSprints = pendingSprintResult.Data.AvailableSprints;

                var parameters = new DialogParameters<CompleteSprintModal>
                {
                    { x => x.BoardId, Guid.Parse(Id) },
                    { x => x.PendingCards, pendingCards },
                    { x => x.AvailableSprints, availableSprints },
                    { x => x.SprintId , sprint.Id },
                };

                var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
                var dialog = _dialogService.Show<CompleteSprintModal>(_localizer["Complete sprint"], parameters, options);

                var result = await dialog.Result;

                if (!result.Canceled && result.Data is bool sprintSucceeded)
                {
                    await _table.ReloadServerData();
                }
            }
        }

        private async Task ViewReports(Guid sprintId)
        {
            var parameters = new DialogParameters<SprintReportsModal>
            {
                {
                    modal => modal.BoardId, Guid.Parse(Id)
                },
                {
                    modal => modal.SprintId, sprintId
                }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Large, FullScreen = false, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<SprintReportsModal>(_localizer["Sprint reports"], parameters, options);
            var result = await dialog.Result;
        }

        private void ViewSprint(SprintSummary sprint)
        {
            _navigationManager.NavigateTo($"/projects/{Id}/sprints/{sprint.Id}");
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
