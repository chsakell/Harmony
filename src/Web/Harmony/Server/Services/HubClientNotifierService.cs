using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Application.Events;
using Harmony.Server.Hubs;
using Harmony.Shared.Constants.Application;
using Microsoft.AspNetCore.SignalR;

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
    }
}
