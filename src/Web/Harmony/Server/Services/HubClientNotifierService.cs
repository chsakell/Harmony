using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Client.Pages.Management;
using Harmony.Domain.Entities;
using Harmony.Server.Hubs;
using Harmony.Shared.Constants.Application;
using Microsoft.AspNetCore.SignalR;
using System.Net.Mail;
using static Harmony.Application.Events.BoardListArchivedEvent;
using static MudBlazor.CategoryTypes;

namespace Harmony.Server.Services
{
    public class HubClientNotifierService : IHubClientNotifierService
    {
        private readonly IHubContext<SignalRHub> _hubContext;

        public HubClientNotifierService(IHubContext<SignalRHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task UpdateBoardListTitle(Guid boardId, Guid boardListId, string title)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnBoardListTitleChanged,
                    new BoardListTitleChangedEvent(boardId, boardListId, title));
        }

        public async Task UpdateCardTitle(Guid boardId, int cardId, string title)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardTitleChanged,
                    new CardTitleChangedEvent(cardId, title));
        }

        public async Task UpdateCardDescription(Guid boardId, int cardId, string description)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardDescriptionChanged,
                    new CardDescriptionChangedEvent(cardId, description));
        }

        public async Task UpdateCardDates(Guid boardId, int cardId, DateTime? startDate, DateTime? dueDate)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardDatesChanged,
                    new CardDatesChangedEvent(cardId, startDate, dueDate));
        }

        public async Task ToggleCardLabel(Guid boardId, int cardId, LabelDto label)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardLabelToggled,
                    new CardLabelToggledEvent(cardId, label));
        }

        public async Task AddCardAttachment(Guid boardId, int cardId, AttachmentDto attachment)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardAttachmentAdded,
                    new AttachmentAddedEvent(cardId, attachment));
        }

        public async Task RemoveCardAttachment(Guid boardId, int cardId, Guid attachmentId)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardAttachmentRemoved,
                    new AttachmentRemovedEvent(cardId, attachmentId));
        }

        public async Task ToggleCardListItemChecked(Guid boardId, int cardId, Guid listItemId, bool isChecked)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardItemChecked,
                    new CardItemCheckedEvent(cardId, listItemId, isChecked));
        }

        public async Task CreateCheckListItem(Guid boardId, int cardId)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardItemAdded,
                    new CardItemAddedEvent(cardId));
        }

        public async Task AddBoardList(Guid boardId, BoardListDto boardList)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnBoardListAdded,
                    new BoardListAddedEvent(boardList));
        }

        public async Task ArchiveBoardList(Guid boardId, Guid archivedList, List<BoardListOrder> positions)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnBoardListArchived,
                    new BoardListArchivedEvent(boardId, archivedList, positions));
        }

        public async Task RemoveCardLabel(Guid boardId, Guid cardLabelId)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardLabelRemoved,
                    new CardLabelRemovedEvent(cardLabelId));
        }

        public async Task UpdateBoardListsPositions(Guid boardId, Dictionary<Guid, short> positions)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnBoardListsPositionsChanged,
                    new BoardListsPositionsChangedEvent(boardId, positions));
        }

        public async Task AddCardMember(Guid boardId, int cardId, CardMemberDto cardMember)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardMemberAdded,
                    new CardMemberAddedEvent(cardId, cardMember));
        }

        public async Task RemoveCardMember(Guid boardId, int cardId, CardMemberDto cardMember)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardMemberRemoved,
                    new CardMemberRemovedEvent(cardId, cardMember));
        }

        public async Task RemoveCheckList(Guid boardId, Guid checkListId, int cardId, int totalItems, int totalItemsCompleted)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCheckListRemoved,
                    new CheckListRemovedEvent(checkListId, cardId, totalItems, totalItemsCompleted));
        }
    }
}
