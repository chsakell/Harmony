using Blazored.LocalStorage;
using Harmony.Application.Events;
using Microsoft.AspNetCore.Components;
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
        Task<HubConnection> StartAsync(NavigationManager navigationManager, ILocalStorageService localStorageService);

        #region Listeners
        Task ListenForBoardEvents(string boardId);
        #endregion

        #region Events

        event EventHandler<CardTitleChangedEvent> OnCardTitleChanged;
        event EventHandler<CardDescriptionChangedEvent> OnCardDescriptionChanged;
        event EventHandler<CardLabelToggledEvent> OnCardLabelToggled;
        event EventHandler<CardDatesChangedEvent> OnCardDatesChanged;
        event EventHandler<AttachmentAddedEvent> OnCardAttachmentAdded;

        event EventHandler<CardItemCheckedEvent> OnCardItemChecked;
        #endregion
    }
}
