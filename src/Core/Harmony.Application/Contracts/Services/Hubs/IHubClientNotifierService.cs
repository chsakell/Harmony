using Harmony.Application.DTO;
using static Harmony.Application.Events.BoardListArchivedEvent;

namespace Harmony.Application.Contracts.Services.Hubs
{
    /// <summary>
    /// Service for Hub client notifications
    /// </summary>
    public interface IHubClientNotifierService
    {
        Task AddBoardList(Guid boardId, BoardListDto boardList);
        Task UpdateBoardListTitle(Guid boardId, Guid boardListId, string title);
        Task UpdateCardTitle(Guid boardId, int cardId, string title);
        Task UpdateCardDescription(Guid boardId, int cardId, string description);
        Task UpdateCardDates(Guid boardId, int cardId, DateTime? startDate, DateTime? dueDate);
        Task ToggleCardLabel(Guid boardId, int cardId, LabelDto label);
        Task AddCardAttachment(Guid boardId, int cardId, AttachmentDto attachment);
        Task RemoveCardAttachment(Guid boardId, int cardId, Guid attachmentId);
        Task CreateCheckListItem(Guid boardId, int cardId);
        Task ToggleCardListItemChecked(Guid boardId, int cardId, Guid listItemId, bool isChecked);
        Task ArchiveBoardList(Guid boardId, Guid archivedList, List<BoardListOrder> positions);
        Task RemoveCardLabel(Guid boardId, Guid cardLabelId);
        Task UpdateBoardListsPositions(Guid boardId, Dictionary<Guid, short> positions);
        Task AddCardMember(Guid boardId, int cardId, CardMemberDto cardMember);
        Task RemoveCardMember(Guid boardId, int cardId, CardMemberDto cardMember);
        Task RemoveCheckList(Guid boardId, Guid checkListId, int cardId, int totalItems, int totalItemsCompleted);
    }
}
