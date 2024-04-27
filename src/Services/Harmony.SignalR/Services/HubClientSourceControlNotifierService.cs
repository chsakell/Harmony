using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Notifications;
using Harmony.Application.SourceControl.Messages;
using Harmony.Application.SourceControl.Services.Hubs;
using Harmony.Shared.Constants.Application;
using Harmony.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Harmony.SignalR.Services
{
    public class HubClientSourceControlNotifierService : IHubClientSourceControlNotifierService
    {
        private readonly IHubContext<SignalRHub> _hubContext;

        public HubClientSourceControlNotifierService(IHubContext<SignalRHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task BranchCreated(BranchCreatedMessage messsage)
        {
            await _hubContext.Clients.Group(messsage.SerialKey)
                .SendAsync(ApplicationConstants.SignalR.OnBranchCreated, messsage);
        }

        public async Task BranchCommitsPushed(BranchCommitsPushedMessage messsage)
        {
            await _hubContext.Clients.Group(messsage.SerialKey)
                .SendAsync(ApplicationConstants.SignalR.OnBranchCommitsPushed, messsage);
        }
    }
}
