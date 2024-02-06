using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.Get;
using static Harmony.Application.Notifications.BoardListArchivedMessage;

namespace Harmony.Client.Infrastructure.Store.Kanban
{
    public interface IKanbanStore : IStore
	{
		GetBoardResponse Board { get; }
		bool BoardLoading { get; }
		void SetLoading(bool loading);
		void Dispose();

        public IEnumerable<BoardListDto> KanbanLists { get; }
		public bool IsScrum { get; }
		public IEnumerable<CardDto> KanbanCards { get; }
		void LoadBoard(GetBoardResponse board);
		void AddListToBoard(BoardListDto list);
		void AddCardToList(CardDto card, BoardListDto list);
		void MoveCard(CardDto card, Guid previousListId, Guid nextListId, short previousPosition, short newPosition);
		void UpdateBoardListTitle(Guid listId, string title);
		void ArchiveList(BoardListDto list);
		void ArchiveListAndReorder(Guid listId, List<BoardListOrder> listPositions);
        void ArchiveCard(Guid cardId);
		void UpdateBoardListCards(Guid listId, List<CardDto> cards);
        void UpdateTodalCardItemsCompleted(Guid cardId, bool increase);
		void UpdateTodalCardChildren(Guid cardId, bool increase);
        void UpdateTodalCardItems(Guid cardId, bool increase);
        void UpdateCardDescription(Guid cardId, string description);
		void UpdateCardStoryPoints(Guid cardId, short? storyPoints);
        void UpdateCardTitle(Guid cardId, string title);
		void ToggleCardLabel(Guid cardId, LabelDto label);
        void UpdateCardDates(Guid cardId, DateTime? startDate, DateTime? dueDate);
		void ChangeTotalCardAttachments(Guid cardId, bool increase);
		void RemoveCardLabel(Guid labelId);
		void ReorderLists(Dictionary<Guid, short> listPositions);
		void AddCardMember(Guid cardId, CardMemberDto cardMember);
		void RemoveCardMember(Guid cardId, CardMemberDto cardMember);
        void ReduceCardProgress(Guid cardId, int totalItems, int totalItemsCompleted);
		void UpdateCardIssueType(Guid cardId, IssueTypeDto issueType);
    }
}
