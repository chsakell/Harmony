using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Commands.CreateList;
using Harmony.Application.Features.Cards.Commands.CreateCard;
using Harmony.Application.Features.Cards.Commands.MoveCard;
using Harmony.Application.Features.Lists.Commands.ArchiveList;
using Harmony.Client.Infrastructure.Store.Kanban;
using Harmony.Client.Shared.Modals;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

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
			}
		}

		private async Task CardMoved(MudItemDropInfo<CardDto> info)
		{
			if(info?.Item == null)
			{
				return;
			};

			var moveToListId = Guid.Parse(info.DropzoneIdentifier);
			var currentListId = info.Item.BoardListId;
			var newPosition = (byte)info.IndexInZone;

			if(moveToListId == currentListId && info.Item.Position == newPosition)
			{
				return;
			}

			var result = await _cardManager
				.MoveCardAsync(new MoveCardCommand(info.Item.Id, moveToListId, newPosition));

			if (result.Succeeded)
			{
				KanbanStore.MoveCard(result.Data, currentListId, moveToListId, newPosition);
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

		private async Task AddCard(BoardListDto list)
		{
			var result = await _cardManager
				.CreateCardAsync(new CreateCardCommand(list.CreateCard.Name, Guid.Parse(Id), list.Id));

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
            var result = await _boardListManager
                .UpdateListStatusAsync(new UpdateListStatusCommand(list.Id, Domain.Enums.BoardListStatus.Archived));

            if (result.Succeeded && result.Data)
            {
                KanbanStore.ArchiveList(list);
                _dropContainer.Refresh();
            }

            DisplayMessage(result);
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
            if (!result.Cancelled)
            {
                // TODO update workspace list or navigate to it
            }
        }


        private void DisplayMessage(IResult result)
		{
			if(result == null)
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
