using Harmony.Application.Events;
using Harmony.Shared.Constants.Application;
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

        public void Init(HubConnection hubConnection)
        {
            _hubConnection = hubConnection;

            RegisterEvents();
        }

        public async Task RegisterBoardEvents(string boardId)
        {
            await _hubConnection.SendAsync(ApplicationConstants.SignalR.RegisterBoardEvents, boardId);
        }

        private void RegisterEvents()
        {
            _hubConnection.On<CardTitleChangedEvent>(ApplicationConstants.SignalR.OnCardTitleChanged, (cardTitleChangedEvent) =>
            {
                Console.WriteLine(cardTitleChangedEvent.Title);
            });
        }
    }
}
