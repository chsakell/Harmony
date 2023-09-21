using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Client.Infrastructure.Models.Kanban;
using Harmony.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Client.Infrastructure.Store.Kanban
{
	public interface IKanbanStore : IStore
	{
		GetBoardResponse Board { get; }
		public List<KanBanList> KanbanLists { get; }
		public List<KanbanListCard> KanbanCards { get; }
		void LoadBoard(GetBoardResponse board);
		void AddListToBoard(BoardListDto list);
		void AddCardToList(CardDto card, Guid listId);
		void MoveCard(CardDto card, Guid previousListId, Guid nextListId, byte newPosition);
		void DeleteList(KanBanList list);
	}
}
