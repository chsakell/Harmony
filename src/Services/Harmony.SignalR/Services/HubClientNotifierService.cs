using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Notifications;
using Harmony.Shared.Constants.Application;
using Harmony.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using static Harmony.Application.Notifications.BoardListArchivedMessage;

namespace Harmony.SignalR.Services
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

        public async Task UpdateCardTitle(Guid boardId, Guid cardId, string title)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardTitleChanged,
                    new CardTitleChangedEvent(cardId, title));
        }

        public async Task UpdateCardDescription(Guid boardId, Guid cardId, string description)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardDescriptionChanged,
                    new CardDescriptionChangedEvent(cardId, description));
        }

        public async Task UpdateCardStoryPoints(Guid boardId, Guid cardId, short? storyPoints)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardStoryPointsChanged,
                    new CardStoryPointsChangedEvent(cardId, storyPoints));
        }

        public async Task UpdateCardDates(Guid boardId, Guid cardId, DateTime? startDate, DateTime? dueDate)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardDatesChanged,
                    new CardDatesChangedEvent(cardId, startDate, dueDate));
        }

        public async Task ToggleCardLabel(Guid boardId, Guid cardId, LabelDto label)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardLabelToggled,
                    new CardLabelToggledEvent(cardId, label));
        }

        public async Task AddCardAttachment(Guid boardId, Guid cardId, AttachmentDto attachment)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardAttachmentAdded,
                    new AttachmentAddedEvent(cardId, attachment));
        }

        public async Task RemoveCardAttachment(Guid boardId, Guid cardId, Guid attachmentId)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardAttachmentRemoved,
                    new AttachmentRemovedEvent(cardId, attachmentId));
        }

        public async Task ToggleCardListItemChecked(Guid boardId, Guid cardId, Guid listItemId, bool isChecked)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardItemChecked,
                    new CardItemCheckedEvent(cardId, listItemId, isChecked));
        }

        public async Task CreateCheckListItem(Guid boardId, Guid cardId)
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
                    new BoardListArchivedMessage(boardId, archivedList, positions));
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

        public async Task AddCardMember(Guid boardId, Guid cardId, CardMemberDto cardMember)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardMemberAdded,
                    new CardMemberAddedEvent(cardId, cardMember));
        }

        public async Task RemoveCardMember(Guid boardId, Guid cardId, CardMemberDto cardMember)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardMemberRemoved,
                    new CardMemberRemovedEvent(cardId, cardMember));
        }

        public async Task RemoveCheckList(Guid boardId, Guid checkListId, Guid cardId, int totalItems, int totalItemsCompleted)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCheckListRemoved,
                    new CheckListRemovedEvent(checkListId, cardId, totalItems, totalItemsCompleted));
        }

        public async Task UpdateCardPosition(Guid boardId, Guid cardId, Guid previousBoardListId, Guid newBoardListId,
            short previousPosition, short newPosition, Guid updateId)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardItemPositionChanged,
                    new CardItemPositionChangedEvent(boardId, cardId, previousBoardListId,
                    newBoardListId, previousPosition, newPosition, updateId));
        }

        public async Task AddCardToBoard(CardCreatedMessage message)
        {
            await _hubContext.Clients.Group(message.BoardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardCreated,
                    new CardCreatedEvent(message));
        }

        public async Task ChangeCardStatus(CardStatusChangedMessage message)
        {
            await _hubContext.Clients.Group(message.BoardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardStatusChanged, message);
        }

        public async Task UpdateCardIssueType(Guid boardId, Guid cardId, IssueTypeDto issueType)
        {
            await _hubContext.Clients.Group(boardId.ToString())
                .SendAsync(ApplicationConstants.SignalR.OnCardIssueTypeChanged,
                    new CardIssueTypeChangedEvent(cardId, issueType));
        }
    }
}
