using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Harmony.Shared.Constants.Application;

namespace Harmony.Server.Hubs
{
    [Authorize]
    public class SignalRHub : Hub
    {
        public async Task OnConnectAsync(string userId)
        {
            await Clients.All.SendAsync(ApplicationConstants.SignalR.ConnectUser, userId);
        }

        public async Task OnDisconnectAsync(string userId)
        {
            await Clients.All.SendAsync(ApplicationConstants.SignalR.DisconnectUser, userId);
        }

        public async Task RegenerateTokensAsync()
        {
            await Clients.All.SendAsync(ApplicationConstants.SignalR.ReceiveRegenerateTokens);
        }

        public async Task ListenForBoardEvents(string boardId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, boardId);
        }
    }
}