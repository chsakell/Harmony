using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.Get;

namespace Harmony.Client.Infrastructure.Store.Kanban
{
    public interface IKanbanStore : IStore
	{
		GetBoardResponse Board { get; }
		bool BoardLoading { get; }
		public IEnumerable<BoardListDto> KanbanLists { get; }
		public IEnumerable<CardDto> KanbanCards { get; }
		void LoadBoard(GetBoardResponse board);
		void AddListToBoard(BoardListDto list);
		void AddCardToList(CardDto card, BoardListDto list);
		void MoveCard(CardDto card, Guid previousListId, Guid nextListId, byte newPosition);
		void ArchiveList(BoardListDto list);
        void ArchiveCard(Guid cardId);
        void UpdateTodalCardItemsCompleted(Guid cardId, bool increase);
        void UpdateTodalCardItems(Guid cardId, bool increase);
        void UpdateCardDescription(Guid cardId, string description);
        void UpdateCardTitle(Guid cardId, string title);
		void ToggleCardLabel(Guid cardId, LabelDto label);
        void UpdateCardDates(Guid cardId, DateTime? startDate, DateTime? dueDate);
		void ChangeTotalCardAttachments(Guid cardId, bool increase);
    }
}
