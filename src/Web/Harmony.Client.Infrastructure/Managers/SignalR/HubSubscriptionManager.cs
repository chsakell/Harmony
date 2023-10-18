using Blazored.LocalStorage;
using Harmony.Application.Events;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Constants.Application;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public event EventHandler<CardTitleChangedEvent> OnCardTitleChanged;

        #endregion

        #region Listeners
        public async Task ListenForBoardEvents(string boardId)
        {
            await _hubConnection.SendAsync(ApplicationConstants.SignalR.ListenForBoardEvents, boardId);
        }
        #endregion

        #region Handlers

        private void HandleEvents()
        {
            _hubConnection.On<CardTitleChangedEvent>(ApplicationConstants.SignalR.OnCardTitleChanged, (cardTitleChangedEvent) =>
            {
                OnCardTitleChanged?.Invoke(this, new CardTitleChangedEvent(cardTitleChangedEvent.CardId, cardTitleChangedEvent.Title));
            });
        }

        #endregion
    }
}
