using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.GetArchivedItems;
using Harmony.Application.Features.Boards.Queries.GetBacklog;
using Harmony.Application.Features.Cards.Commands.CreateBacklog;
using Harmony.Application.Features.Cards.Commands.MoveCard;
using Harmony.Application.Features.Cards.Commands.UpdateBacklog;
using Harmony.Application.Features.Workspaces.Queries.GetBacklog;
using Harmony.Client.Infrastructure.Models.Board;
using Harmony.Client.Shared.Modals;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;

namespace Harmony.Client.Pages.Management
{
    public partial class ArchivedItems : IDisposable
    {
        [Parameter]
        public string Id { get; set; }

        private string _searchString = "";
        private int _totalItems;
        private List<GetArchivedItemResponse> _cards;
        private MudTable<GetArchivedItemResponse> _table;
        private HashSet<GetArchivedItemResponse> _selectedCards = new HashSet<GetArchivedItemResponse>();
        private GetArchivedItemResponse _itemBeforeEdit;
        private List<IssueTypeDto> _issueTypes;
        private IDisposable registration;

        protected override async Task OnInitializedAsync()
        {
            await _hubSubscriptionManager.ListenForBoardEvents(Id);
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
            }
        }

        private async Task Reactivate()
        {
            if(!_selectedCards.Any()) 
            { 
                return; 
            }

            var boardType = _selectedCards.FirstOrDefault().BoardType;
            switch (boardType)
            {
                case Domain.Enums.BoardType.Kanban:
                    var parameters = new DialogParameters<ReactivateCardsModal>
                    {
                        {
                            modal => modal.BoardId,Guid.Parse(Id)
                        },
                        {
                            modal => modal.Items, _selectedCards
                        }
                    };

                    var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
                    var dialog = _dialogService.Show<ReactivateCardsModal>(_localizer["Reactivate cards"], parameters, options);
                    var result = await dialog.Result;

                    if (!result.Canceled)
                    {
                        await _table.ReloadServerData();
                    }
                    break;
                        case Domain.Enums.BoardType.Scrum:
                    var moveSprintParams = new DialogParameters<ReactivateScrumCardsModal>
                    {
                        {
                            modal => modal.BoardId,Guid.Parse(Id)
                        },
                        {
                            modal => modal.Items, _selectedCards
                        }
                    };

                    var moveSprintOptions = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true, DisableBackdropClick = true };
                    var moveSprintOptionsDialog = _dialogService.Show<ReactivateScrumCardsModal>(_localizer["Reactivate cards"], moveSprintParams, moveSprintOptions);
                    var moveSprintResult = await moveSprintOptionsDialog.Result;

                    if (!moveSprintResult.Canceled)
                    {
                        await _table.ReloadServerData();
                    }
                    break;
            }
            
        }

        private async Task<TableData<GetArchivedItemResponse>> ReloadData(TableState state)
        {
            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                state.Page = 0;
            }
            await LoadData(state.Page, state.PageSize, state);

            return new TableData<GetArchivedItemResponse>
            {
                TotalItems = _totalItems,
                Items = _cards
            };
        }

        private async Task EditCard(GetArchivedItemResponse card)
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

        private void EditCardModal_OnCardUpdated(object? sender, EditableCardModel e, GetArchivedItemResponse item)
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

            var request = new GetArchivedItemsQuery(Guid.Parse(Id))
            {
                BoardId = Guid.Parse(Id),
                PageSize = pageSize,
                PageNumber = pageNumber + 1,
                SearchTerm = _searchString,
                OrderBy = orderings
            };

            var response = await _boardManager.GetArchivedItems(request);
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
            var item = element as GetArchivedItemResponse;

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
                Id = ((GetArchivedItemResponse)element).Id,
                Title = ((GetArchivedItemResponse)element).Title,
                DueDate = ((GetArchivedItemResponse)element).DueDate,
                IssueType = ((GetArchivedItemResponse)element).IssueType,
                SerialKey = ((GetArchivedItemResponse)element).SerialKey,
                StoryPoints = ((GetArchivedItemResponse)element).StoryPoints,
                Position = ((GetArchivedItemResponse)element).Position,
                StartDate = ((GetArchivedItemResponse)element).StartDate
            };
        }

        private void CancelEdit(object element)
        {
            ((GetArchivedItemResponse)element).Id = _itemBeforeEdit.Id;
            ((GetArchivedItemResponse)element).Title = _itemBeforeEdit.Title;
            ((GetArchivedItemResponse)element).DueDate = _itemBeforeEdit.DueDate;
            ((GetArchivedItemResponse)element).StartDate = _itemBeforeEdit.StartDate;
            ((GetArchivedItemResponse)element).Position = _itemBeforeEdit.Position;
            ((GetArchivedItemResponse)element).IssueType = _itemBeforeEdit.IssueType;
            ((GetArchivedItemResponse)element).StoryPoints = _itemBeforeEdit.StoryPoints;
            ((GetArchivedItemResponse)element).SerialKey = _itemBeforeEdit.SerialKey;
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
