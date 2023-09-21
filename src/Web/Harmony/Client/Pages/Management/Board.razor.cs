using Harmony.Application.Features.Boards.Commands.CreateList;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetAllForUser;
using Harmony.Application.Features.Cards.Commands.CreateCard;
using Harmony.Application.Features.Cards.Commands.MoveCard;
using Harmony.Client.Infrastructure.Models.Kanban;
using Harmony.Shared.Utilities;
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

		private GetBoardResponse _board = new GetBoardResponse();

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
				_board = result.Data;

				foreach (var list in _board.Lists)
				{
					_kanbanLists.Add(new KanBanList(list.Id, list.Name, list.Position));

					foreach(var card in list.Cards.OrderBy(c => c.Position))
					{
						_kanbanCards.Add(new KanbanListCard(card.Id, list.Id, card.Name, card.Position));
					}
				}
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

			var cardId = info.Item.Id;
			var moveToListId = Guid.Parse(info.DropzoneIdentifier);
			var currentListId = info.Item.BoardListId;

			var currentPosition = info.Item.Position;
			var newPosition = (byte)info.IndexInZone;

			var result = await _cardManager
				.MoveCardAsync(new MoveCardCommand(info.Item.Id, moveToListId, newPosition));

			if (result.Succeeded && result.Messages.Any())
			{
				info.Item.BoardListId = moveToListId;
				info.Item.Position = newPosition;

				var cardAdded = result.Data;

				var currentList = _board.Lists.FirstOrDefault(l => l.Id == currentListId);
				var currentCard = currentList?.Cards.FirstOrDefault(c => c.Id == cardId);

				if(currentCard != null)
				{
					if(currentListId != moveToListId)
					{
						currentList.Cards.Remove(currentCard);
						
						var newBoardList = _board.Lists.Find(l => l.Id == cardAdded.BoardListId);
						var cardsInNewBoardListGreaterOrEqualThanIndex = newBoardList.Cards.Where(c => c.Position >= cardAdded.Position);
						
						// needs increasing by 1
						foreach (var cardInNewBoardList in cardsInNewBoardListGreaterOrEqualThanIndex)
						{
							cardInNewBoardList.Position += 1;
						}

						newBoardList.Cards.Add(cardAdded);
					}
					else
					{
						// needs swapping
						if(currentCard.Position != newPosition)
						{
							var currentCardInIndex = currentList.Cards.FirstOrDefault(c => c.Position == newPosition);
							if(currentCardInIndex != null)
							{
								currentCardInIndex.Position = currentCard.Position;
							}
						}

						currentCard.Position = newPosition;
					}
				}

				_snackBar.Add(result.Messages[0], Severity.Success);
			}
			else
			{
				foreach (var message in result.Messages)
				{
					_snackBar.Add(message, Severity.Error);
				}
			}

			info.Item.BoardListId = Guid.Parse(info.DropzoneIdentifier);
		}

		private async Task OnValidListSubmit(EditContext context)
		{
			var result = await _boardManager.CreateListAsync(new CreateListCommand(_newListModel.Name, Guid.Parse(Id)));

			if(result.Succeeded && result.Messages.Any())
			{
				var listAdded = result.Data;
				_board.Lists.Add(listAdded);
				
				_kanbanLists.Add(new KanBanList(listAdded.Id, listAdded.Name, listAdded.Position));
				_newListModel.Name = string.Empty;
				_addNewListOpen = false;

				_snackBar.Add(result.Messages[0], Severity.Success);
			}
			else
			{
				foreach (var message in result.Messages)
				{
					_snackBar.Add(message, Severity.Error);
				}
			}
		}

		private void OpenAddNewList()
		{
			_addNewListOpen = true;
		}

		private async Task AddCard(KanBanList kanBanList)
		{
			var result = await _cardManager
				.CreateCardAsync(new CreateCardCommand(kanBanList.NewCardName, Guid.Parse(Id), kanBanList.Id));

			if (result.Succeeded && result.Messages.Any())
			{
				var cardAdded = result.Data;
				var boardList = _board.Lists.Find(l => l.Id == kanBanList.Id);
				boardList.Cards.Add(cardAdded);

				_kanbanCards.Add(new KanbanListCard(cardAdded.Id, cardAdded.BoardListId, kanBanList.NewCardName, cardAdded.Position));
				
				kanBanList.NewCardName = string.Empty;
				kanBanList.NewTaskOpen = false;
				_dropContainer.Refresh();

				_snackBar.Add(result.Messages[0], Severity.Success);
			}
			else
			{
				foreach (var message in result.Messages)
				{
					_snackBar.Add(message, Severity.Error);
				}
			}
		}

		private void DeleteList(KanBanList section)
		{
			if (_kanbanLists.Count == 1)
			{
				_kanbanCards.Clear();
				_kanbanLists.Clear();
			}
			else
			{
				int newIndex = _kanbanLists.IndexOf(section) - 1;
				if (newIndex < 0)
				{
					newIndex = 0;
				}

				_kanbanLists.Remove(section);

				var tasks = _kanbanCards.Where(x => x.BoardListId == section.Id);

				foreach (var item in tasks)
				{
					//item.Status = _kanbanLists[newIndex].Name;
				}
			}
		}
		#endregion
	}
}
