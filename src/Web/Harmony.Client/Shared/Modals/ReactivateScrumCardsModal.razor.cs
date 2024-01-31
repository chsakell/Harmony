using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.GetArchivedItems;
using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Features.Cards.Commands.MoveToSprint;
using Harmony.Application.Features.Lists.Queries.GetBoardLists;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class ReactivateScrumCardsModal
    {
        private bool _processing;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public Guid BoardId { get; set; }

        [Parameter]
        public HashSet<GetArchivedItemResponse> Items { get; set; }

        private MudTable<SprintDto> _table;
        private string _searchString = "";
        private List<SprintDto> _sprints;
        private int _totalItems;
        private List<GetBoardListResponse> _boardLists = new List<GetBoardListResponse>();
        private GetBoardListResponse _selectedBoardList;

        private int selectedRowNumber = -1;
        private void Cancel()
        {
            MudDialog.Cancel();
        }

        protected override async Task OnInitializedAsync()
        {
            var boardListsResult = await _boardManager
                .GetBoardListsAsync(BoardId.ToString());

            if ((boardListsResult.Succeeded))
            {
                _boardLists = boardListsResult.Data.OrderBy(l => l.Position).ToList();

                if (_boardLists.Any())
                {
                    _selectedBoardList = _boardLists.First();
                }
            }
        }

        private async Task MoveCards()
        {
            var selectedSprint = _table.SelectedItem;

            if (selectedSprint == null)
            {
                _snackBar.Add("Please select a sprint first.", Severity.Warning);

                return;
            }

            _processing = true;

            var result = await _boardManager
                .MoveCardsToSprint(new MoveToSprintCommand(BoardId, selectedSprint.Id,
                _selectedBoardList.Id, Items.Select(i => i.Id).ToList()));

            DisplayMessage(result);

            _processing = false;

            if (result.Succeeded)
            {
                MudDialog.Close(result.Data);
            }
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

            var request = new GetSprintsQuery(BoardId)
            {
                PageSize = pageSize,
                PageNumber = pageNumber + 1,
                SearchTerm = _searchString,
                OrderBy = orderings,
                Statuses = new List<SprintStatus> { SprintStatus.Idle, SprintStatus.Active }
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

        private void RowClickEvent(TableRowClickEventArgs<SprintDto> tableRowClickEventArgs)
        {

        }

        private string SelectedRowClassFunc(SprintDto sprint, int rowNumber)
        {
            if (selectedRowNumber == rowNumber)
            {
                selectedRowNumber = -1;

                _table.SetSelectedItem(null);

                return string.Empty;
            }
            else if (_table.SelectedItem != null && _table.SelectedItem.Equals(sprint))
            {
                selectedRowNumber = rowNumber;

                return "mud-harmony";
            }
            else
            {
                return string.Empty;
            }
        }


        Func<GetBoardListResponse, string> converter = p =>
        {
            return p?.Title ?? "Move to list";
        };

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
