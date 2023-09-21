using Harmony.Application.Features.Boards.Commands.CreateList;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetAllForUser;
using Harmony.Application.Features.Cards.Commands.CreateCard;
using Harmony.Application.Features.Cards.Commands.MoveCard;
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
		private List<KanBanList> _kanbanLists = new();
		public IEnumerable<KanbanListCard> KanbanCards => _kanbanCards.OrderBy(c => c.Position);

		private List<KanbanListCard> _kanbanCards = new();
		private MudDropContainer<KanbanListCard> _dropContainer;
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

		#region helper
		
		/* handling board events */
		private async Task TaskUpdated(MudItemDropInfo<KanbanListCard> info)
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
				info.Item.BoardListId = moveToListId;
				info.Item.Position = newPosition;

				KanbanStore.MoveCard(result.Data, currentListId, moveToListId, newPosition);
			}

			DisplayMessage(result);

			info.Item.BoardListId = Guid.Parse(info.DropzoneIdentifier);
		}

		private async Task OnValidListSubmit(EditContext context)
		{
			var result = await _boardManager.CreateListAsync(new CreateListCommand(_newListModel.Name, Guid.Parse(Id)));

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

		private async Task AddCard(KanBanList kanBanList)
		{
			var result = await _cardManager
				.CreateCardAsync(new CreateCardCommand(kanBanList.NewCardName, Guid.Parse(Id), kanBanList.Id));

			if (result.Succeeded)
			{
				var cardAdded = result.Data;
				
				KanbanStore.AddCardToList(cardAdded, kanBanList.Id);

				kanBanList.NewCardName = string.Empty;
				kanBanList.NewTaskOpen = false;
				_dropContainer.Refresh();
			}

			DisplayMessage(result);
		}

		private void DeleteList(KanBanList section)
		{
			KanbanStore.DeleteList(section);

			//if (_kanbanLists.Count == 1)
			//{
			//	_kanbanCards.Clear();
			//	_kanbanLists.Clear();
			//}
			//else
			//{
			//	int newIndex = _kanbanLists.IndexOf(section) - 1;
			//	if (newIndex < 0)
			//	{
			//		newIndex = 0;
			//	}

			//	_kanbanLists.Remove(section);

			//	var tasks = _kanbanCards.Where(x => x.BoardListId == section.Id);

			//	foreach (var item in tasks)
			//	{
			//		item.Name = _kanbanLists[newIndex].Name;
			//	}
			//}
		}
		#endregion

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
