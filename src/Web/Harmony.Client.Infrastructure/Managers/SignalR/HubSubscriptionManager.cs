using Blazored.LocalStorage;
using Harmony.Application.Events;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Constants.Application;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

namespace Harmony.Client.Infrastructure.Managers.SignalR
{
    public class HubSubscriptionManager : IHubSubscriptionManager
    {
        private HubConnection _hubConnection;
        public bool IsConnected => _hubConnection.State == HubConnectionState.Connected;

        public async Task<HubConnection> StartAsync(NavigationManager navigationManager, ILocalStorageService localStorageService)
        {
            _hubConnection = _hubConnection.TryInitialize(navigationManager, localStorageService);
            await _hubConnection.StartAsync();

            HandleEvents();

            return _hubConnection;
        }

        #region Events

        public event EventHandler<BoardListAddedEvent> OnBoardListAdded;
        public event EventHandler<BoardListArchivedEvent> OnBoardListArchived;
        public event EventHandler<CardTitleChangedEvent> OnCardTitleChanged;
        public event EventHandler<CardDescriptionChangedEvent> OnCardDescriptionChanged;
        public event EventHandler<CardLabelToggledEvent> OnCardLabelToggled;
        public event EventHandler<CardDatesChangedEvent> OnCardDatesChanged;
        public event EventHandler<AttachmentAddedEvent> OnCardAttachmentAdded;
        public event EventHandler<CardItemCheckedEvent> OnCardItemChecked;
        public event EventHandler<CardItemAddedEvent> OnCardItemAdded;

        #endregion

        #region Listeners
        public async Task ListenForBoardEvents(string boardId)
        {
            await _hubConnection.SendAsync(ApplicationConstants.SignalR.ListenForBoardEvents, boardId);
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

            _hubConnection.On<BoardListArchivedEvent>(ApplicationConstants.SignalR.OnBoardListArchived, (@event) =>
            {
                OnBoardListArchived?.Invoke(this,
                    new BoardListArchivedEvent(@event.BoardId, @event.ArchivedList, @event.Positions));
            });
        }

        #endregion
    }
}
