using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.GetArchivedItems;
using Harmony.Application.Features.Boards.Queries.GetWorkItems;
using Harmony.Application.Features.Cards.Commands.UpdateBacklog;
using Harmony.Application.Models;
using Harmony.Client.Infrastructure.Helper;
using Harmony.Client.Infrastructure.Models.Board;
using Harmony.Client.Shared.Modals;
using Harmony.Domain.Entities;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;

namespace Harmony.Client.Pages.Management
{
    public partial class WorkItems : IDisposable
    {
        [Parameter]
        public string Id { get; set; }

        private string _searchString = "";
        private int _totalItems;
        private List<CardDto> _cards;
        private MudTable<CardDto> _table;
        private HashSet<CardDto> _selectedCards = new HashSet<CardDto>();
        private CardDto _itemBeforeEdit;
        private List<IssueTypeDto> _issueTypes;
        private IDisposable registration;
        private BoardInfo? _boardInfo = new BoardInfo();
        private bool _reloadData = false;

        private IEnumerable<IssueTypeDto> _selectedIssueTypes = new List<IssueTypeDto>();
        private IEnumerable<BoardListDto> _selectedLists = new List<BoardListDto>();
        private IEnumerable<SprintDto> _selectedSprints = new List<SprintDto>();

        protected async override Task OnParametersSetAsync()
        {
            await _hubSubscriptionManager.ListenForBoardEvents(Id);

            var boardInfoResult = await _boardManager.GetBoardInfoAsync(Id);

            if (boardInfoResult.Succeeded)
            {
                _boardInfo = boardInfoResult.Data;
            }

            if (_reloadData)
            {
                await _table.ReloadServerData();
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                registration = _navigationManager.RegisterLocationChangingHandler(LocationChangingHandler);
            }
        }

        private async ValueTask LocationChangingHandler(LocationChangingContext arg)
        {
            if (!arg.TargetLocation.Contains(Id))
            {
                await _hubSubscriptionManager.StopListeningForBoardEvents(Id);
                _reloadData = true;

                Clear();
            }
        }

        private void Clear()
        {
            _selectedIssueTypes = new List<IssueTypeDto>();
            _selectedLists = new List<BoardListDto>();
            _selectedSprints = new List<SprintDto>();
        }

        private string GetMultiSelectionIssueTypesText(List<string> selectedValues)
        {
            if (!_selectedIssueTypes.Any())
            {
                return string.Empty;
            }

            return string.Join(", ", _selectedIssueTypes.Select(x => x.Summary));
        }

        private string GetMultiSelectionBoardListsText(List<string> selectedValues)
        {
            if (!_selectedLists.Any())
            {
                return string.Empty;
            }

            return string.Join(", ", _selectedLists.Select(x => x.Title));
        }

        private string GetMultiSelectionSprintsText(List<string> selectedValues)
        {
            if (!_selectedLists.Any())
            {
                return string.Empty;
            }

            return string.Join(", ", _selectedSprints.Select(x => x.Name));
        }

        private void SetSelectedIssueTypes(IEnumerable<IssueTypeDto> newSelectedIssueTypes)
        {

            _selectedIssueTypes = newSelectedIssueTypes;

            _table.ReloadServerData();
        }

        private void SetSelectedLists(IEnumerable<BoardListDto> newSelectedLists)
        {
            _selectedLists = newSelectedLists;

            _table.ReloadServerData();
        }

        private void SetSelectedSprints(IEnumerable<SprintDto> newSelectedSprints)
        {
            _selectedSprints = newSelectedSprints;

            _table.ReloadServerData();
        }

        private string GetBoardList(Guid boardListId)
        {
            var list = _boardInfo.Lists.FirstOrDefault(l => l.Id == boardListId);

            if (list == null)
            {
                return "N/A";
            }

            return list.Title;
        }

        private Color GetBoardListColor(Guid boardListId)
        {
            var list = _boardInfo?.Lists.FirstOrDefault(l => l.Id == boardListId);

            return ColorHelper.GetBoardListColor(list);
        }

        private async Task<TableData<CardDto>> ReloadData(TableState state)
        {
            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                state.Page = 0;
            }
            await LoadData(state.Page, state.PageSize, state);

            _reloadData = false;

            return new TableData<CardDto>
            {
                TotalItems = _totalItems,
                Items = _cards
            };
        }

        private async Task EditCard(CardDto card)
        {
            var parameters = new DialogParameters<EditCardModal>
                {
                    { c => c.CardId, card.Id },
                    { c => c.BoardId, Guid.Parse(Id) },
                    { c => c.BoardKey, card.BoardKey }
                };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true, DisableBackdropClick = false };
            var dialog = await _dialogService.ShowAsync<EditCardModal>(_localizer["Edit card"], parameters, options);

            var editCardModal = dialog.Dialog as EditCardModal;

            editCardModal.OnCardUpdated += (object? sender, EditableCardModel e) => EditCardModal_OnCardUpdated(sender, e, card);

            var result = await dialog.Result;

            editCardModal.OnCardUpdated -= (object? sender, EditableCardModel e) => EditCardModal_OnCardUpdated(sender, e, card); ;
        }

        private void EditCardModal_OnCardUpdated(object? sender, EditableCardModel e, CardDto item)
        {
            item.Title = e.Title;
            item.StoryPoints = e.StoryPoints;
            item.DueDate = e.DueDate;
            item.IssueType = e.IssueType;
        }

        private Func<IssueTypeDto, string> convertFunc = type =>
        {
            if (type == null || type.Id == Guid.Empty)
            {
                return "Select issue type";
            }

            return type.Summary;
        };

        private async Task LoadData(int pageNumber, int pageSize, TableState state)
        {
            string[] orderings = null;
            if (!string.IsNullOrEmpty(state.SortLabel))
            {
                orderings = state.SortDirection != SortDirection.None ? new[] { $"{state.SortLabel} {state.SortDirection}" } : new[] { $"{state.SortLabel}" };
            }
            else
            {
                orderings = new[] { $"DateCreated descending" };
            }

            var request = new GetWorkItemsQuery(Guid.Parse(Id))
            {
                PageSize = pageSize,
                PageNumber = pageNumber + 1,
                CardTitle = _searchString,
                OrderBy = orderings,
                BoardLists = _selectedLists.Select(l => l.Id).ToList(),
                IssueTypes = _selectedIssueTypes.Select(i => i.Id).ToList(),
                Sprints = _selectedSprints.Select(i => i.Id).ToList()
            };

            var response = await _boardManager.GetWorkItems(request);

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


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadIssueTypes();
            }
        }

        private async Task LoadIssueTypes()
        {
            if (_issueTypes != null)
            {
                return;
            }

            var issueTypesResult = await _boardManager.GetIssueTypesAsync(Id);

            if (issueTypesResult.Succeeded)
            {
                _issueTypes = issueTypesResult.Data;
            }
        }

        #region Row Editing

        private async void UpdateItem(object element)
        {
            var item = element as CardDto;

            if (item == null)
            {
                return;
            }

            var result = await _cardManager
                .UpdateBacklogItemAsync(new
                UpdateBacklogCommand(item.Id, Guid.Parse(Id), item.Title, item.IssueType, item.StoryPoints));

            DisplayMessage(result);
        }

        private void BackupItem(object element)
        {
            _itemBeforeEdit = new()
            {
                Id = ((CardDto)element).Id,
                Title = ((CardDto)element).Title,
                DueDate = ((CardDto)element).DueDate,
                IssueType = ((CardDto)element).IssueType,
                SerialNumber = ((CardDto)element).SerialNumber,
                StoryPoints = ((CardDto)element).StoryPoints,
                Position = ((CardDto)element).Position,
                StartDate = ((CardDto)element).StartDate
            };
        }

        private void CancelEdit(object element)
        {
            ((CardDto)element).Id = _itemBeforeEdit.Id;
            ((CardDto)element).Title = _itemBeforeEdit.Title;
            ((CardDto)element).DueDate = _itemBeforeEdit.DueDate;
            ((CardDto)element).StartDate = _itemBeforeEdit.StartDate;
            ((CardDto)element).Position = _itemBeforeEdit.Position;
            ((CardDto)element).IssueType = _itemBeforeEdit.IssueType;
            ((CardDto)element).StoryPoints = _itemBeforeEdit.StoryPoints;
            ((CardDto)element).SerialNumber = _itemBeforeEdit.SerialNumber;
        }

        #endregion

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

        void IDisposable.Dispose()
        {
            registration?.Dispose();
        }
    }
}
