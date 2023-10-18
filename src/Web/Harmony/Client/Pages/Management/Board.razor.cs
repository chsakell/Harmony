using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Features.Boards.Commands.CreateList;
using Harmony.Application.Features.Cards.Commands.CreateCard;
using Harmony.Application.Features.Cards.Commands.MoveCard;
using Harmony.Application.Features.Cards.Commands.UpdateCardStatus;
using Harmony.Application.Features.Lists.Commands.ArchiveList;
using Harmony.Client.Infrastructure.Store.Kanban;
using Harmony.Client.Shared.Dialogs;
using Harmony.Client.Shared.Modals;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using static Harmony.Shared.Constants.Permission.Permissions;

namespace Harmony.Client.Pages.Management
{
    public partial class Board
    {
        [Parameter]
        public string Id { get; set; }

        [Parameter]
        public string Name { get; set; }

        [Inject]
        public IKanbanStore KanbanStore { get; set; }

        private MudDropContainer<CardDto> _dropContainer;

        protected async override Task OnInitializedAsync()
        {
            var result = await _boardManager.GetBoardAsync(Id);

            if (result.Succeeded)
            {
                KanbanStore.LoadBoard(result.Data);

                _checkListItemManager.OnCardItemChecked += CheckListItemManager_OnCardItemChecked;
                _checkListManager.OnCardItemAdded += CheckListManager_OnCardItemAdded;
                _hubSubscriptionManager.OnCardDescriptionChanged += OnCardDescriptionChanged;
                _hubSubscriptionManager.OnCardTitleChanged += OnCardTitleChanged;
                _hubSubscriptionManager.OnCardLabelToggled += OnCardLabelToggled;
                _hubSubscriptionManager.OnCardDatesChanged += OnCardDatesChanged;
                _fileManager.OnCardAttachmentAdded += FileManager_OnCardAttachmentAdded;

                await _hubSubscriptionManager.ListenForBoardEvents(Id);
            }
        }

        private void FileManager_OnCardAttachmentAdded(object? sender, AttachmentAddedEvent e)
        {
            KanbanStore.ChangeTotalCardAttachments(e.CardId, true);

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

        private void CheckListManager_OnCardItemAdded(object? sender, CardItemAddedEvent e)
        {
            KanbanStore.UpdateTodalCardItems(e.CardId, increase: true);

            _dropContainer.Refresh();
        }

        private void CheckListItemManager_OnCardItemChecked(object? sender, CardItemCheckedEvent e)
        {
            KanbanStore.UpdateTodalCardItemsCompleted(e.CardId, e.IsChecked);

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

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
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

        private async Task OpenShareBoardModal()
        {
            var parameters = new DialogParameters<BoardMembersModal>
            {
                {
                    modal => modal.BoardId, Guid.Parse(Id)
                }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<BoardMembersModal>(_localizer["Share board"], parameters, options);
            var result = await dialog.Result;
        }

        private async Task AddCard(BoardListDto list)
        {
            var result = await _cardManager
                .CreateCardAsync(new CreateCardCommand(list.CreateCard.Title, Guid.Parse(Id), list.Id));

            if (result.Succeeded)
            {
                var cardAdded = result.Data;

                KanbanStore.AddCardToList(cardAdded, list);
                _dropContainer.Refresh();
            }

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
                { c => c.CardId, card.Id }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true, DisableBackdropClick = true };
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
    }
}
