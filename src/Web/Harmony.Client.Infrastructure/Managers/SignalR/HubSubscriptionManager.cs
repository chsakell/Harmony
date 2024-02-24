using Blazored.LocalStorage;
using Harmony.Application.Events;
using Harmony.Application.Notifications;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Constants.Application;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Polly;
using Polly.Registry;
using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Managers.SignalR
{
    /// <summary>
    /// Manager for connecting via SignalR
    /// Subscribes to board events and invoke event handlers to subscribers
    /// </summary>
    public class HubSubscriptionManager : IHubSubscriptionManager
    {
        private HubConnection _hubConnection;
        public bool IsConnected => _hubConnection.State == HubConnectionState.Connected;

        public async Task<HubConnection> StartAsync(NavigationManager navigationManager, 
            ILocalStorageService localStorageService, string signalrHostUrl,
            ResiliencePipeline pipeline)
        {
            _hubConnection = _hubConnection
                .Get(navigationManager, localStorageService, signalrHostUrl);

            await pipeline.ExecuteAsync(async token =>
            {
                await Start();
            });

            HandleEvents();

            return _hubConnection;
        }

        private async Task Start()
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
                await _hubConnection.StartAsync();
        }

        public async Task StopAsync()
        {
            await _hubConnection.StopAsync();
        }

        #region Events

        public event EventHandler<BoardListAddedEvent> OnBoardListAdded;
        public event EventHandler<BoardListTitleChangedEvent> OnBoardListTitleChanged;
        public event EventHandler<BoardListArchivedMessage> OnBoardListArchived;
        public event EventHandler<BoardListsPositionsChangedEvent> OnBoardListsPositionsChanged;
        public event EventHandler<CardTitleChangedEvent> OnCardTitleChanged;
        public event EventHandler<CardIssueTypeChangedEvent> OnCardIssueTypeChanged;
        public event EventHandler<CardDescriptionChangedEvent> OnCardDescriptionChanged;
        public event EventHandler<CardStoryPointsChangedEvent> OnCardStoryPointsChanged;
        public event EventHandler<CardLabelToggledEvent> OnCardLabelToggled;
        public event EventHandler<CardDatesChangedEvent> OnCardDatesChanged;
        public event EventHandler<AttachmentAddedEvent> OnCardAttachmentAdded;
        public event EventHandler<CardItemPositionChangedEvent> OnCardItemPositionChanged;
        public event EventHandler<AttachmentRemovedEvent> OnCardAttachmentRemoved;
        public event EventHandler<CardItemCheckedEvent> OnCardItemChecked;
        public event EventHandler<CardItemAddedEvent> OnCardItemAdded;
        public event EventHandler<CardLabelRemovedEvent> OnCardLabelRemoved;
        public event EventHandler<CardMemberAddedEvent> OnCardMemberAdded;
        public event EventHandler<CardMemberRemovedEvent> OnCardMemberRemoved;
        public event EventHandler<CheckListRemovedEvent> OnCheckListRemoved;
        public event EventHandler<CardCreatedEvent> OnCardCreated;
        public event EventHandler<CardStatusChangedMessage> OnCardStatusChanged;

        #endregion

        #region Listeners
        public async Task ListenForBoardEvents(string boardId)
        {
            await _hubConnection.SendAsync(ApplicationConstants.SignalR.ListenForBoardEvents, boardId);
        }

        public async Task StopListeningForBoardEvents(string boardId)
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                try
                {
                    await _hubConnection.SendAsync(ApplicationConstants.SignalR.StopListeningForBoardEvents, boardId);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
        }

        #endregion

        #region Event Handlers

        private void HandleEvents()
        {
            HandleCardChanges();
        }

        private void HandleCardChanges()
        {
            _hubConnection.On<CardTitleChangedEvent>(ApplicationConstants.SignalR.OnCardTitleChanged, (@event) =>
            {
                OnCardTitleChanged?.Invoke(this, new CardTitleChangedEvent(@event.CardId, @event.Title));
            });

            _hubConnection.On<CardDescriptionChangedEvent>(ApplicationConstants.SignalR.OnCardDescriptionChanged, (@event) =>
            {
                OnCardDescriptionChanged?.Invoke(this, new CardDescriptionChangedEvent(@event.CardId, @event.Description));
            });

            _hubConnection.On<CardStoryPointsChangedEvent>(ApplicationConstants.SignalR.OnCardStoryPointsChanged, (@event) =>
            {
                OnCardStoryPointsChanged?.Invoke(this, @event);
            });

            _hubConnection.On<CardDatesChangedEvent>(ApplicationConstants.SignalR.OnCardDatesChanged, (@event) =>
            {
                OnCardDatesChanged?.Invoke(this, new CardDatesChangedEvent(@event.CardId, @event.StartDate, @event.DueDate));
            });

            _hubConnection.On<CardLabelToggledEvent>(ApplicationConstants.SignalR.OnCardLabelToggled, (@event) =>
            {
                OnCardLabelToggled?.Invoke(this, new CardLabelToggledEvent(@event.CardId, @event.Label));
            });

            _hubConnection.On<AttachmentAddedEvent>(ApplicationConstants.SignalR.OnCardAttachmentAdded, (@event) =>
            {
                OnCardAttachmentAdded?.Invoke(this,
                    new AttachmentAddedEvent(@event.CardId, @event.Attachment));
            });

            _hubConnection.On<CardIssueTypeChangedEvent>(ApplicationConstants.SignalR.OnCardIssueTypeChanged, (@event) =>
            {
                OnCardIssueTypeChanged?.Invoke(this, @event);
            });

            _hubConnection.On<AttachmentRemovedEvent>(ApplicationConstants.SignalR.OnCardAttachmentRemoved, (@event) =>
            {
                OnCardAttachmentRemoved?.Invoke(this,
                    new AttachmentRemovedEvent(@event.CardId, @event.AttachmentId));
            });

            _hubConnection.On<CardItemCheckedEvent>(ApplicationConstants.SignalR.OnCardItemChecked, (@event) =>
            {
                OnCardItemChecked?.Invoke(this, new CardItemCheckedEvent(@event.CardId, @event.CheckListItemId, @event.IsChecked));
            });

            _hubConnection.On<CardItemAddedEvent>(ApplicationConstants.SignalR.OnCardItemAdded, (@event) =>
            {
                OnCardItemAdded?.Invoke(this, new CardItemAddedEvent(@event.CardId));
            });

            _hubConnection.On<BoardListAddedEvent>(ApplicationConstants.SignalR.OnBoardListAdded, (@event) =>
            {
                OnBoardListAdded?.Invoke(this, new BoardListAddedEvent(@event.BoardList));
            });

            _hubConnection.On<BoardListArchivedMessage>(ApplicationConstants.SignalR.OnBoardListArchived, (@event) =>
            {
                OnBoardListArchived?.Invoke(this,
                    new BoardListArchivedMessage(@event.BoardId, @event.ArchivedList, @event.Positions));
            });

            _hubConnection.On<BoardListTitleChangedEvent>(ApplicationConstants.SignalR.OnBoardListTitleChanged, (@event) =>
            {
                OnBoardListTitleChanged?.Invoke(this,
                    new BoardListTitleChangedEvent(@event.BoardId, @event.BoardListId, @event.Title));
            });

            _hubConnection.On<CardLabelRemovedEvent>(ApplicationConstants.SignalR.OnCardLabelRemoved, (@event) =>
            {
                OnCardLabelRemoved?.Invoke(this, new CardLabelRemovedEvent(@event.CardLabelId));
            });

            _hubConnection.On<BoardListsPositionsChangedEvent>(ApplicationConstants.SignalR.OnBoardListsPositionsChanged, (@event) =>
            {
                OnBoardListsPositionsChanged?.Invoke(this, new BoardListsPositionsChangedEvent(@event.BoardId, @event.ListPositions));
            });

            _hubConnection.On<CardMemberAddedEvent>(ApplicationConstants.SignalR.OnCardMemberAdded, (@event) =>
            {
                OnCardMemberAdded?.Invoke(this, new CardMemberAddedEvent(@event.CardId, @event.Member));
            });

            _hubConnection.On<CardMemberRemovedEvent>(ApplicationConstants.SignalR.OnCardMemberRemoved, (@event) =>
            {
                OnCardMemberRemoved?.Invoke(this, new CardMemberRemovedEvent(@event.CardId, @event.Member));
            });

            _hubConnection.On<CheckListRemovedEvent>(ApplicationConstants.SignalR.OnCheckListRemoved, (@event) =>
            {
                OnCheckListRemoved?.Invoke(this, new CheckListRemovedEvent(@event.CheckListId, @event.CardId, @event.TotalItems, @event.TotalItemsCompleted));
            });

            _hubConnection.On<CardItemPositionChangedEvent>(ApplicationConstants.SignalR.OnCardItemPositionChanged, (@event) =>
            {
                OnCardItemPositionChanged?.Invoke(this, @event);
            });

            _hubConnection.On<CardCreatedEvent>(ApplicationConstants.SignalR.OnCardCreated, (@event) =>
            {
                OnCardCreated?.Invoke(this, @event);
            });

            _hubConnection.On<CardStatusChangedMessage>(ApplicationConstants.SignalR.OnCardStatusChanged, (@event) =>
            {
                OnCardStatusChanged?.Invoke(this, @event);
            });
        }

        #endregion
    }
}
