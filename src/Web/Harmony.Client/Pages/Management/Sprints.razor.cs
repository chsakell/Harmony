
using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Features.Boards.Queries.GetSprintsDetails;
using Harmony.Application.Features.Workspaces.Commands.AddMember;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
using Harmony.Client.Shared.Dialogs;
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

        private List<SprintDetails> _sprints;
        private MudTable<SprintDetails> _table;
        private string _searchString = "";
        private int _totalItems;
        public SprintStatus? _status { get; set; } = null;

        private bool _loading;


        private async Task<TableData<SprintDetails>> ReloadData(TableState state)
        {
            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                state.Page = 0;
            }
            await LoadData(state.Page, state.PageSize, state);

            return new TableData<SprintDetails>
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

            var request = new GetSprintsDetailsQuery(Guid.Parse(Id))
            {
                BoardId = Guid.Parse(Id),
                PageSize = pageSize,
                PageNumber = pageNumber + 1,
                SearchTerm = _searchString,
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
