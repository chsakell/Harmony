using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.Get;
using static Harmony.Application.Events.BoardListArchivedEvent;

namespace Harmony.Client.Infrastructure.Store.Kanban
{
    public interface IKanbanStore : IStore
	{
		GetBoardResponse Board { get; }
		bool BoardLoading { get; }
		void SetLoading(bool loading);
		void Dispose();

        public IEnumerable<BoardListDto> KanbanLists { get; }
		public IEnumerable<CardDto> KanbanCards { get; }
		void LoadBoard(GetBoardResponse board);
		void AddListToBoard(BoardListDto list);
		void AddCardToList(CardDto card, BoardListDto list);
		void MoveCard(CardDto card, Guid previousListId, Guid nextListId, byte newPosition);
		void UpdateBoardListTitle(Guid listId, string title);
		void ArchiveList(BoardListDto list);
		void ArchiveListAndReorder(Guid listId, List<BoardListOrder> listPositions);
        void ArchiveCard(int cardId);
		void UpdateBoardListCards(Guid listId, List<CardDto> cards);
        void UpdateTodalCardItemsCompleted(int cardId, bool increase);
        void UpdateTodalCardItems(int cardId, bool increase);
        void UpdateCardDescription(int cardId, string description);
        void UpdateCardTitle(int cardId, string title);
		void ToggleCardLabel(int cardId, LabelDto label);
        void UpdateCardDates(int cardId, DateTime? startDate, DateTime? dueDate);
		void ChangeTotalCardAttachments(int cardId, bool increase);
		void RemoveCardLabel(Guid labelId);
		void ReorderLists(Dictionary<Guid, short> listPositions);
		void AddCardMember(int cardId, CardMemberDto cardMember);
		void RemoveCardMember(int cardId, CardMemberDto cardMember);
        void ReduceCardProgress(int cardId, int totalItems, int totalItemsCompleted);
    }
}
