using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Client.Infrastructure.Managers.Project;
using Harmony.Client.Infrastructure.Models.Kanban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Harmony.Client.Infrastructure.Store.Kanban
{
	public class KanbanStore : IKanbanStore
	{
		private GetBoardResponse _board = new GetBoardResponse();
		private List<KanBanList> _kanbanLists = new();
		private List<KanbanListCard> _kanbanCards = new();

		public GetBoardResponse Board => _board;
		public List<KanBanList> KanbanLists => _kanbanLists;
		public List<KanbanListCard> KanbanCards => _kanbanCards;


		public void LoadBoard(GetBoardResponse board)
		{
			_board = board;

			foreach (var list in board.Lists)
			{
				_kanbanLists.Add(new KanBanList(list.Id, list.Name, list.Position));

				foreach (var card in list.Cards.OrderBy(c => c.Position))
				{
					_kanbanCards.Add(new KanbanListCard(card.Id, list.Id, card.Name, card.Position));
				}
			}
		}

		public void AddListToBoard(BoardListDto list)
		{
			_board.Lists.Add(list);

			_kanbanLists.Add(new KanBanList(list.Id, list.Name, list.Position));
		}

		public void AddCardToList(CardDto card, Guid listId)
		{
			var boardList = _board.Lists.Find(l => l.Id == listId);
			if (boardList == null)
			{
				return;
			}

			boardList.Cards.Add(card);

			_kanbanCards.Add(new KanbanListCard(card.Id, card.BoardListId, card.Name, card.Position));
		}

		public void MoveCard(CardDto card, Guid previousListId, Guid nextListId,  byte newPosition)
		{
			var currentList = _board.Lists.FirstOrDefault(l => l.Id == previousListId);
			var currentCard = currentList?.Cards.FirstOrDefault(c => c.Id == card.Id);

			if (currentCard != null)
			{
				if (previousListId != nextListId)
				{
					currentList.Cards.Remove(currentCard);

					var newBoardList = _board.Lists.Find(l => l.Id == card.BoardListId);
					var cardsInNewBoardListGreaterOrEqualThanIndex = newBoardList.Cards.Where(c => c.Position >= card.Position);

					// needs increasing by 1
					foreach (var cardInNewBoardList in cardsInNewBoardListGreaterOrEqualThanIndex)
					{
						cardInNewBoardList.Position += 1;
					}

					newBoardList.Cards.Add(card);
				}
				else
				{
					// needs swapping
					if (currentCard.Position != newPosition)
					{
						var currentCardInIndex = currentList.Cards.FirstOrDefault(c => c.Position == newPosition);
						if (currentCardInIndex != null)
						{
							currentCardInIndex.Position = currentCard.Position;
						}
					}

					currentCard.Position = newPosition;
				}
			}
		}

		public void DeleteList(KanBanList list)
		{
			_kanbanLists.RemoveAll(l => l.Id == list.Id);
			_kanbanCards.RemoveAll(c => c.BoardListId == list.Id);
		}
	}
}
