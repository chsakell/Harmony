using AutoMapper.Execution;
using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Commands.CreateSprint;
using Harmony.Application.Features.Boards.Queries.GetBacklog;
using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Features.Cards.Commands.CreateBacklog;
using Harmony.Application.Features.Cards.Commands.MoveToSprint;
using Harmony.Application.Features.Lists.Commands.CreateList;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;

namespace Harmony.Client.Shared.Modals
{
    public partial class MoveToSprintModal
    {
        private bool _processing;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public MoveToSprintCommand MoveToSprintCommandModel { get; set; }

        [Parameter]
        public HashSet<GetBacklogItemResponse> Items { get; set; }

        private MudTable<SprintDto> _table;
        private string _searchString = "";
        private List<SprintDto> _sprints;
        private int _totalItems;

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task<TableData<SprintDto>> ReloadData(TableState state)
        {
            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                state.Page = 0;
            }
            await LoadData(state.Page, state.PageSize, state);

            return new TableData<SprintDto>
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

            var request = new GetSprintsQuery(MoveToSprintCommandModel.BoardId)
            {
                PageSize = pageSize,
                PageNumber = pageNumber + 1,
                SearchTerm = _searchString,
                OrderBy = orderings,
            };

            var response = await _boardManager.GetSprints(request);

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
