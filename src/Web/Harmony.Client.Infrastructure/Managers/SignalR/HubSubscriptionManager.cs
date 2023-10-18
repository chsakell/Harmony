using Harmony.Application.Events;
using Harmony.Shared.Constants.Application;
using MediatR;
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

        #region Events
        public event EventHandler<CardTitleChangedEvent> OnCardTitleChanged;
        #endregion

        public void Init(HubConnection hubConnection)
        {
            _hubConnection = hubConnection;

            HandleEvents();
        }

        public async Task ListenForBoardEvents(string boardId)
        {
            await _hubConnection.SendAsync(ApplicationConstants.SignalR.ListenForBoardEvents, boardId);
        }

        private void HandleEvents()
        {
            _hubConnection.On<CardTitleChangedEvent>(ApplicationConstants.SignalR.OnCardTitleChanged, (cardTitleChangedEvent) =>
            {
                OnCardTitleChanged?.Invoke(this, new CardTitleChangedEvent(cardTitleChangedEvent.CardId, cardTitleChangedEvent.Title));
            });
        }
    }
}
