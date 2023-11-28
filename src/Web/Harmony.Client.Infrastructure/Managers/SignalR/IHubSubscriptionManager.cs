using Blazored.LocalStorage;
using Harmony.Application.Events;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Harmony.Client.Infrastructure.Managers.SignalR
{
    public interface IHubSubscriptionManager : IManager
    {
        Task<HubConnection> StartAsync(NavigationManager navigationManager, ILocalStorageService localStorageService);
        Task StopAsync();

        #region Listeners
        Task ListenForBoardEvents(string boardId);
        Task StopListeningForBoardEvents(string boardId);
        #endregion

        #region Events
        event EventHandler<BoardListAddedEvent> OnBoardListAdded;
        event EventHandler<BoardListTitleChangedEvent> OnBoardListTitleChanged;
        event EventHandler<BoardListArchivedEvent> OnBoardListArchived;
        event EventHandler<BoardListsPositionsChangedEvent> OnBoardListsPositionsChanged;
        event EventHandler<CardTitleChangedEvent> OnCardTitleChanged;
        event EventHandler<CardDescriptionChangedEvent> OnCardDescriptionChanged;
        event EventHandler<CardLabelToggledEvent> OnCardLabelToggled;
        event EventHandler<CardDatesChangedEvent> OnCardDatesChanged;
        event EventHandler<AttachmentAddedEvent> OnCardAttachmentAdded;
        event EventHandler<AttachmentRemovedEvent> OnCardAttachmentRemoved;
        event EventHandler<CardItemAddedEvent> OnCardItemAdded;
        event EventHandler<CardItemPositionChangedEvent> OnCardItemPositionChanged;
        event EventHandler<CardItemCheckedEvent> OnCardItemChecked;
        event EventHandler<CardLabelRemovedEvent> OnCardLabelRemoved;
        event EventHandler<CardMemberAddedEvent> OnCardMemberAdded;
        event EventHandler<CardMemberRemovedEvent> OnCardMemberRemoved;
        event EventHandler<CheckListRemovedEvent> OnCheckListRemoved;

        #endregion
    }
}
