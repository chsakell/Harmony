using Blazored.LocalStorage;
using Harmony.Application.Events;
using Harmony.Application.Notifications;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Polly;

namespace Harmony.Client.Infrastructure.Managers.SignalR
{
    public interface IHubSubscriptionManager : IManager
    {
        Task<HubConnection> StartAsync(NavigationManager navigationManager, 
            ILocalStorageService localStorageService,
            string signalrHostUrl, ResiliencePipeline pipeline);
        Task StopAsync();

        #region Listeners
        Task ListenForBoardEvents(string boardId);
        Task StopListeningForBoardEvents(string boardId);
        #endregion

        #region Events
        event EventHandler<BoardListAddedEvent> OnBoardListAdded;
        event EventHandler<BoardListTitleChangedEvent> OnBoardListTitleChanged;
        event EventHandler<BoardListArchivedMessage> OnBoardListArchived;
        event EventHandler<BoardListsPositionsChangedEvent> OnBoardListsPositionsChanged;
        event EventHandler<CardTitleChangedEvent> OnCardTitleChanged;
        event EventHandler<CardIssueTypeChangedEvent> OnCardIssueTypeChanged;
        event EventHandler<CardDescriptionChangedEvent> OnCardDescriptionChanged;
        event EventHandler<CardStoryPointsChangedEvent> OnCardStoryPointsChanged;
        event EventHandler<CardLabelToggledEvent> OnCardLabelToggled;
        event EventHandler<CardDatesChangedEvent> OnCardDatesChanged;
        event EventHandler<AttachmentAddedEvent> OnCardAttachmentAdded;
        event EventHandler<CardItemPositionChangedEvent> OnCardItemPositionChanged;
        event EventHandler<AttachmentRemovedEvent> OnCardAttachmentRemoved;
        event EventHandler<CardItemAddedEvent> OnCardItemAdded;
        event EventHandler<CardItemCheckedEvent> OnCardItemChecked;
        event EventHandler<CardLabelRemovedEvent> OnCardLabelRemoved;
        event EventHandler<CardMemberAddedEvent> OnCardMemberAdded;
        event EventHandler<CardMemberRemovedEvent> OnCardMemberRemoved;
        event EventHandler<CheckListRemovedEvent> OnCheckListRemoved;
        event EventHandler<CardCreatedEvent> OnCardCreated;
        event EventHandler<CardStatusChangedMessage> OnCardStatusChanged;

        #endregion
    }
}
