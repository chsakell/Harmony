using Harmony.Application.Events;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Client.Infrastructure.Managers.SignalR
{
    public interface IHubSubscriptionManager : IManager
    {
        void Init(HubConnection hubConnection);
        Task ListenForBoardEvents(string boardId);

        #region Events
        event EventHandler<CardTitleChangedEvent> OnCardTitleChanged;
        #endregion
    }
}
