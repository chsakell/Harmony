using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Server.Hubs;
using Harmony.Shared.Constants.Application;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;

namespace Harmony.Server.Services
{
    public class HubClientNotifierService : IHubClientNotifierService
    {
        private readonly IHubContext<SignalRHub> _hubContext;

        public HubClientNotifierService(IHubContext<SignalRHub> hubContext)
        {
            _hubContext = hubContext;
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
    }
}
