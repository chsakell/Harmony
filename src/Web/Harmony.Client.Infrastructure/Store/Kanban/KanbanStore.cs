using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Client.Infrastructure.Models.Kanban;


namespace Harmony.Client.Infrastructure.Store.Kanban
{
	public class KanbanStore : IKanbanStore
	{
		private GetBoardResponse _board = new GetBoardResponse();

		private bool _boardLoading = true;
		public bool BoardLoading => _boardLoading;
		public GetBoardResponse Board => _board;
		public IEnumerable<BoardListDto> KanbanLists => _board.Lists.OrderBy(l => l.Position);
		public IEnumerable<CardDto> KanbanCards => _board.Lists.SelectMany(l => l.Cards).OrderBy(c => c.Position);

		public void LoadBoard(GetBoardResponse board)
		{
			_board = board;

			_boardLoading = false;
		}

		public void AddListToBoard(BoardListDto list)
		{
			_board.Lists.Add(list);
		}

		public void AddCardToList(CardDto card, BoardListDto list)
		{
			var boardList = _board.Lists.Find(l => l.Id == list.Id);

			if (boardList == null)
			{
				return;
			}

			boardList.Cards.Add(card);
            list.NewCardName = string.Empty;
            list.NewTaskOpen = false;
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

		public void ArchiveList(BoardListDto list)
		{
			_board.Lists.RemoveAll(l => l.Id == list.Id);
		}
	}
}
