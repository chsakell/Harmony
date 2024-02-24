using Microsoft.AspNetCore.SignalR;

namespace Harmony.SignalR.Hubs
{
    public class SignalRHub : Hub
    {
        public async Task ListenForBoardEvents(string boardId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, boardId);
        }

        public async Task StopListeningForBoardEvents(string boardId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, boardId);
        }
    }
}