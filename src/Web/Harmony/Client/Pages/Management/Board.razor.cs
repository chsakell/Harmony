using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Features.Cards.Commands.CreateCard;
using Harmony.Application.Features.Cards.Commands.MoveCard;
using Harmony.Application.Features.Cards.Commands.UpdateCardStatus;
using Harmony.Application.Features.Lists.Commands.ArchiveList;
using Harmony.Application.Features.Lists.Commands.CreateList;
using Harmony.Application.Features.Lists.Commands.UpdateListsPositions;
using Harmony.Application.Features.Lists.Commands.UpdateListTitle;
using Harmony.Application.Features.Lists.Queries.LoadBoardList;
using Harmony.Client.Infrastructure.Models.Board;
using Harmony.Client.Infrastructure.Store.Kanban;
using Harmony.Client.Shared.Dialogs;
using Harmony.Client.Shared.Modals;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using static MudBlazor.CategoryTypes;

namespace Harmony.Client.Pages.Management
{
    public partial class Board : IAsyncDisposable
    {
        [Parameter]
        public string Id { get; set; }

        [Parameter]
        public string Name { get; set; }

        [Inject]
        public IKanbanStore KanbanStore { get; set; }

        [Inject] private IJSRuntime JSRuntime { get; set; }

        private MudDropContainer<CardDto> _dropContainer;
        public bool CardDescriptionVisibility { get; set; }
        private bool _unauthorisedAccess = false;
        private int _listCardsSize = 10;

        protected async override Task OnInitializedAsync()
        {
            var result = await _boardManager.GetBoardAsync(Id, _listCardsSize);

            if (result.Succeeded)
            {
                KanbanStore.LoadBoard(result.Data);

                await RegisterBoardEvents();
            }
            else
            {
                KanbanStore.SetLoading(false);
                _unauthorisedAccess = result.Code == ResultCode.UnauthorisedAccess;

                DisplayMessage(result);
            }
        }

        private async Task RegisterBoardEvents()
        {
            _hubSubscriptionManager.OnBoardListAdded += OnBoardListAdded;
            _hubSubscriptionManager.OnBoardListTitleChanged += OnBoardListTitleChanged;
            _hubSubscriptionManager.OnBoardListArchived += OnBoardListArchived;
            _hubSubscriptionManager.OnBoardListsPositionsChanged += OnBoardListsPositionsChanged;
            _hubSubscriptionManager.OnCardItemChecked += OnCardItemChecked;
            _hubSubscriptionManager.OnCardItemAdded += OnCardItemAdded;
            _hubSubscriptionManager.OnCardDescriptionChanged += OnCardDescriptionChanged;
            _hubSubscriptionManager.OnCardTitleChanged += OnCardTitleChanged;
            _hubSubscriptionManager.OnCardLabelToggled += OnCardLabelToggled;
            _hubSubscriptionManager.OnCardDatesChanged += OnCardDatesChanged;
            _hubSubscriptionManager.OnCardAttachmentAdded += OnCardAttachmentAdded;
            _hubSubscriptionManager.OnCardAttachmentRemoved += OnCardAttachmentRemoved;
            _hubSubscriptionManager.OnCardLabelRemoved += OnCardLabelRemoved;
            _hubSubscriptionManager.OnCardMemberAdded += OnCardMemberAdded;
            _hubSubscriptionManager.OnCardMemberRemoved += OnCardMemberRemoved;
            _hubSubscriptionManager.OnCheckListRemoved += OnCheckListRemoved;

            await _hubSubscriptionManager.ListenForBoardEvents(Id);
        }

        private async Task UnRegisterBoardEvents()
        {
            _hubSubscriptionManager.OnBoardListAdded -= OnBoardListAdded;
            _hubSubscriptionManager.OnBoardListTitleChanged -= OnBoardListTitleChanged;
            _hubSubscriptionManager.OnBoardListArchived -= OnBoardListArchived;
            _hubSubscriptionManager.OnBoardListsPositionsChanged -= OnBoardListsPositionsChanged;
            _hubSubscriptionManager.OnCardItemChecked -= OnCardItemChecked;
            _hubSubscriptionManager.OnCardItemAdded -= OnCardItemAdded;
            _hubSubscriptionManager.OnCardDescriptionChanged -= OnCardDescriptionChanged;
            _hubSubscriptionManager.OnCardTitleChanged -= OnCardTitleChanged;
            _hubSubscriptionManager.OnCardLabelToggled -= OnCardLabelToggled;
            _hubSubscriptionManager.OnCardDatesChanged -= OnCardDatesChanged;
            _hubSubscriptionManager.OnCardAttachmentAdded -= OnCardAttachmentAdded;
            _hubSubscriptionManager.OnCardAttachmentRemoved -= OnCardAttachmentRemoved;
            _hubSubscriptionManager.OnCardLabelRemoved -= OnCardLabelRemoved;
            _hubSubscriptionManager.OnCardMemberAdded -= OnCardMemberAdded;
            _hubSubscriptionManager.OnCardMemberRemoved -= OnCardMemberRemoved;
            _hubSubscriptionManager.OnCheckListRemoved -= OnCheckListRemoved;

            await _hubSubscriptionManager.StopListeningForBoardEvents(Id);
        }

        private void OnCheckListRemoved(object? sender, CheckListRemovedEvent e)
        {
            KanbanStore.ReduceCardProgress(e.CardId, e.TotalItems, e.TotalItemsCompleted);
            _dropContainer.Refresh();
        }

        private void OnCardMemberAdded(object? sender, CardMemberAddedEvent e)
        {
            KanbanStore.AddCardMember(e.CardId, e.Member);
            _dropContainer.Refresh();
        }

        private void OnCardMemberRemoved(object? sender, CardMemberRemovedEvent e)
        {
            KanbanStore.RemoveCardMember(e.CardId, e.Member);
            _dropContainer.Refresh();
        }

        private void OnBoardListsPositionsChanged(object? sender, BoardListsPositionsChangedEvent e)
        {
            KanbanStore.ReorderLists(e.ListPositions);
            StateHasChanged();
        }

        private void OnCardLabelRemoved(object? sender, CardLabelRemovedEvent e)
        {
            KanbanStore.RemoveCardLabel(e.CardLabelId);
            StateHasChanged();
        }

        private void OnBoardListTitleChanged(object? sender, BoardListTitleChangedEvent e)
        {
            KanbanStore.UpdateBoardListTitle(e.BoardListId, e.Title);
            StateHasChanged();
        }

        private void OnBoardListArchived(object? sender, BoardListArchivedEvent e)
        {
            KanbanStore.ArchiveListAndReorder(e.ArchivedList, e.Positions);
            StateHasChanged();
        }

        private void OnBoardListAdded(object? sender, BoardListAddedEvent e)
        {
            KanbanStore.AddListToBoard(e.BoardList);
            StateHasChanged();
        }

        private void OnCardAttachmentAdded(object? sender, AttachmentAddedEvent e)
        {
            KanbanStore.ChangeTotalCardAttachments(e.CardId, true);

            _dropContainer.Refresh();
        }

        private void OnCardAttachmentRemoved(object? sender, AttachmentRemovedEvent e)
        {
            KanbanStore.ChangeTotalCardAttachments(e.CardId, false);

            _dropContainer.Refresh();
        }

        private void OnCardDatesChanged(object? sender, CardDatesChangedEvent e)
        {
            KanbanStore.UpdateCardDates(e.CardId, e.StartDate, e.DueDate);

            _dropContainer.Refresh();
        }

        private void OnCardLabelToggled(object? sender, CardLabelToggledEvent e)
        {
            KanbanStore.ToggleCardLabel(e.CardId, e.Label);

            _dropContainer.Refresh();
        }

        private void OnCardTitleChanged(object? sender, CardTitleChangedEvent e)
        {
            KanbanStore.UpdateCardTitle(e.CardId, e.Title);

            _dropContainer.Refresh();
        }

        private void OnCardDescriptionChanged(object? sender, CardDescriptionChangedEvent e)
        {
            KanbanStore.UpdateCardDescription(e.CardId, e.Description);

            _dropContainer.Refresh();
        }

        private void OnCardItemAdded(object? sender, CardItemAddedEvent e)
        {
            KanbanStore.UpdateTodalCardItems(e.CardId, increase: true);

            _dropContainer.Refresh();
        }

        private void OnCardItemChecked(object? sender, CardItemCheckedEvent e)
        {
            KanbanStore.UpdateTodalCardItemsCompleted(e.CardId, e.IsChecked);

            _dropContainer.Refresh();
        }

        private async Task SaveBoardListTitle(Guid listId, string title)
        {
            var result = await _boardListManager
                .UpdateBoardListTitleAsync(new UpdateListTitleCommand(Guid.Parse(Id), listId, title));

            if (result.Succeeded)
            {
                KanbanStore.UpdateBoardListTitle(listId, title);
            }

            DisplayMessage(result);
        }

        private async Task LoadListCards(Guid listId, int page)
        {
            var result = await _boardManager
                .GetBoardListCardsAsync(new LoadBoardListQuery(Guid.Parse(Id), listId, page, _listCardsSize));

            if (result.Succeeded)
            {
                KanbanStore.UpdateBoardListCards(listId, result.Data);
                _dropContainer.Refresh();
            }
        }

        private void ToggleCardDescriptionVisibility(bool toggle)
        {
            CardDescriptionVisibility = toggle;
            _dropContainer.Refresh();
        }

        private async Task CardMoved(MudItemDropInfo<CardDto> info)
        {
            if (info?.Item == null)
            {
                return;
            };

            var moveToListId = Guid.Parse(info.DropzoneIdentifier);
            var currentListId = info.Item.BoardListId;
            var newPosition = (byte)info.IndexInZone;

            if (moveToListId == currentListId && info.Item.Position == newPosition)
            {
                return;
            }

            var result = await _cardManager
                .MoveCardAsync(new MoveCardCommand(info.Item.Id, moveToListId, newPosition));

            if (result.Succeeded)
            {
                var cardDto = result.Data;
                cardDto.Labels = info.Item.Labels;
                cardDto.TotalItems = info.Item.TotalItems;
                cardDto.TotalItemsCompleted = info.Item.TotalItemsCompleted;
                cardDto.TotalAttachments = info.Item.TotalAttachments;
                cardDto.Members = info.Item.Members;

                KanbanStore.MoveCard(cardDto, currentListId, moveToListId, newPosition);

                if (currentListId != moveToListId)
                {
                    _dropContainer.Refresh();
                }
            }

            DisplayMessage(result);
        }

        private async Task OpenCreateBoardListModal()
        {
            var parameters = new DialogParameters<CreateBoardListModal>
            {
                {
                    modal => modal.CreateListCommandModel,
                    new CreateListCommand(null, Guid.Parse(Id))
                }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = false, DisableBackdropClick = true };
            var dialog = _dialogService.Show<CreateBoardListModal>(_localizer["Create board list"], parameters, options);
            var result = await dialog.Result;
            if (!result.Canceled)
            {
                var createdList = result.Data as BoardListDto;
                if (createdList != null)
                {
                    KanbanStore.AddListToBoard(createdList);
                }
            }
        }

        private async Task ReorderLists()
        {
            var parameters = new DialogParameters<ReorderBoardListsModal>
            {
                {
                    modal => modal.BoardId, Guid.Parse(Id)
                },
                {
                    modal => modal.Lists, KanbanStore.KanbanLists
                    .OrderBy(l => l.Position).Select(list => new OrderedBoardListModel()
                    {
                        Id = list.Id,
                        Position = list.Position,
                        Title = list.Title
                    }).ToList()
                }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<ReorderBoardListsModal>(_localizer["Reorder lists"], parameters, options);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Canceled &&
                dialogResult.Data is UpdateListsPositionsResponse positions)
            {

                KanbanStore.ReorderLists(positions.ListPositions);
                _dropContainer.Refresh();
            }
        }

        private void ViewBacklog()
        {
            _navigationManager.NavigateTo($"/projects/{Id}/backlog");
        }

        private void ViewSprints()
        {
            _navigationManager.NavigateTo($"/projects/{Id}/sprints");
        }

        private async Task OpenShareBoardModal()
        {
            var parameters = new DialogParameters<BoardMembersModal>
            {
                {
                    modal => modal.BoardId, Guid.Parse(Id)
                }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<BoardMembersModal>(_localizer["Share board"], parameters, options);
            var result = await dialog.Result;
        }

        private async Task AddCard(BoardListDto list)
        {
            list.CreateCard.Creating = true;

            var result = await _cardManager
                .CreateCardAsync(new CreateCardCommand(list.CreateCard.Title, Guid.Parse(Id), list.Id));

            if (result.Succeeded)
            {
                var cardAdded = result.Data;

                KanbanStore.AddCardToList(cardAdded, list);
                _dropContainer.Refresh();

                await JSRuntime.InvokeVoidAsync("scrollToElement", cardAdded.Id.ToString());
            }

            list.CreateCard.Creating = false;

            DisplayMessage(result);
        }

        private async Task ArchiveList(BoardListDto list)
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to archive this list?" },
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Error }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Canceled)
            {
                var result = await _boardListManager
                    .UpdateListStatusAsync(new UpdateListStatusCommand(list.Id, Domain.Enums.BoardListStatus.Archived));

                if (result.Succeeded && result.Data)
                {
                    KanbanStore.ArchiveList(list);
                    _dropContainer.Refresh();
                }

                DisplayMessage(result);
            }
        }

        private async Task EditCard(CardDto card)
        {
            var parameters = new DialogParameters<EditCardModal>
            {
                { c => c.CardId, card.Id },
                { c => c.BoardId, Guid.Parse(Id) },
                { c => c.SerialKey, $"{KanbanStore.Board.Key}-{card.SerialNumber}" }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true, DisableBackdropClick = false };
            var dialog = _dialogService.Show<EditCardModal>(_localizer["Edit card"], parameters, options);
            var result = await dialog.Result;

            if (result.Data is UpdateCardStatusCommand command &&
                command.Status == Domain.Enums.CardStatus.Archived)
            {
                KanbanStore.ArchiveCard(command.CardId);
                _dropContainer.Refresh();
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

        //public async Task DisposeAsync()
        //{
        //    KanbanStore.Dispose();
        //}

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            KanbanStore.Dispose();
            await UnRegisterBoardEvents();
        }
    }
}
