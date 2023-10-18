﻿using Harmony.Application.DTO;

namespace Harmony.Application.Contracts.Services.Hubs
{
    public interface IHubClientNotifierService
    {
        Task AddBoardList(Guid boardId, BoardListDto boardList);
        Task UpdateCardTitle(Guid boardId, Guid cardId, string title);
        Task UpdateCardDescription(Guid boardId, Guid cardId, string description);
        Task UpdateCardDates(Guid boardId, Guid cardId, DateTime? startDate, DateTime? dueDate);
        Task ToggleCardLabel(Guid boardId, Guid cardId, LabelDto label);
        Task AddCardAttachment(Guid boardId, Guid cardId, AttachmentDto attachment);
        Task CreateCheckListItem(Guid boardId, Guid cardId);
        Task ToggleCardListItemChecked(Guid boardId, Guid cardId, Guid listItemId, bool isChecked);
    }
}