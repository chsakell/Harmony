using AutoMapper.Execution;
using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Commands.CreateSprint;
using Harmony.Application.Features.Boards.Queries.GetBacklog;
using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Features.Cards.Commands.CreateBacklog;
using Harmony.Application.Features.Lists.Commands.CreateList;
using Harmony.Application.Features.Workspaces.Queries.GetBacklog;
using Harmony.Application.Features.Workspaces.Queries.GetSprints;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
using Harmony.Client.Infrastructure.Store.Kanban;
using Harmony.Client.Shared.Modals;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static MudBlazor.CategoryTypes;

namespace Harmony.Client.Pages.Management
{
    public partial class Sprints
    {
        [Parameter]
        public string Id { get; set; }

        private string _searchString = "";
        private int _totalItems;
        private List<GetSprintCardResponse> _cards = new List<GetSprintCardResponse>();
        private MudTable<GetSprintCardResponse> _table;

        private TableGroupDefinition<GetSprintCardResponse> _groupDefinition = new()
        {
            GroupName = "Sprint",
            Indentation = false,
            Expandable = true,
            IsInitiallyExpanded = true,
            Selector = (e) => e.Sprint + 
            $" [{e.SprintStartDate.ToShortDateString()}-{e.SprintEndDate.ToShortDateString()}]"
        };

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        private async Task<TableData<GetSprintCardResponse>> ReloadData(TableState state)
        {
            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                state.Page = 0;
            }
            await LoadData(state.Page, state.PageSize, state);

            return new TableData<GetSprintCardResponse>
            {
                TotalItems = _totalItems,
                Items = _cards
            };
        }

        private async Task CreateSprint()
        {
            var parameters = new DialogParameters<CreateSprintModal>
            {
                {
                    modal => modal.CreateSprintCommandModel,
                    new CreateSprintCommand(Guid.Parse(Id))
                }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<CreateSprintModal>(_localizer["Create sprint"], parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await _table.ReloadServerData();
            }
        }

        private async Task LoadData(int pageNumber, int pageSize, TableState state)
        {
            string[] orderings = null;
            if (!string.IsNullOrEmpty(state.SortLabel))
            {
                orderings = state.SortDirection != SortDirection.None ? new[] { $"{state.SortLabel} {state.SortDirection}" } : new[] { $"{state.SortLabel}" };
            }

            var request = new GetSprintCardsQuery(Guid.Parse(Id))
            {
                BoardId = Guid.Parse(Id),
                PageSize = pageSize,
                PageNumber = pageNumber + 1,
                SearchTerm = _searchString,
                OrderBy = orderings
            };

            var response = await _boardManager.GetSprintCards(request);

            if (response.Succeeded)
            {
                _totalItems = response.TotalCount;
                _cards = response.Data;
            }
            else
            {
                foreach (var message in response.Messages)
                {
                    _snackBar.Add(message, Severity.Error);
                }
            }
        }

        private void OnSearch(string text)
        {
            _searchString = string.IsNullOrEmpty(text) ? null : text;
            _table.ReloadServerData();
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
