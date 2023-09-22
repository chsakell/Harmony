using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Commands.CreateList;
using Harmony.Application.Features.Cards.Commands.CreateCard;
using Harmony.Application.Features.Cards.Commands.MoveCard;
using Harmony.Application.Features.Lists.Commands.ArchiveList;
using Harmony.Client.Infrastructure.Models.Kanban;
using Harmony.Client.Infrastructure.Store.Kanban;
using Harmony.Domain.Entities;
using Harmony.Shared.Utilities;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

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

		#region Kanban

		KanBanNewListForm _newListModel = new KanBanNewListForm();

		private MudDropContainer<CardDto> _dropContainer;
		private bool _addNewListOpen;

		#endregion

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

			var result = await _cardManager
				.MoveCardAsync(new MoveCardCommand(info.Item.Id, moveToListId, newPosition));

			if (result.Succeeded)
			{
				KanbanStore.MoveCard(result.Data, currentListId, moveToListId, newPosition);
			}

			DisplayMessage(result);
		}

		private async Task OnValidListSubmit(EditContext context)
		{
			var result = await _boardListManager.CreateListAsync(new CreateListCommand(_newListModel.Name, Guid.Parse(Id)));

			if(result.Succeeded)
			{
				KanbanStore.AddListToBoard(result.Data);

				_newListModel.Name = string.Empty;
				_addNewListOpen = false;
			}
			
			DisplayMessage(result);
		}

		private void OpenAddNewList()
		{
			_addNewListOpen = true;
		}

		private async Task AddCard(BoardListDto list)
		{
			var result = await _cardManager
				.CreateCardAsync(new CreateCardCommand(list.NewCardName, Guid.Parse(Id), list.Id));

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
