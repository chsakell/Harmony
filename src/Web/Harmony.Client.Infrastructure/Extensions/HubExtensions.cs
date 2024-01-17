using Microsoft.AspNetCore.Components;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Harmony.Shared.Constants.Application;

namespace Harmony.Client.Infrastructure.Extensions
{
    public static class HubExtensions
    {
        public static HubConnection TryInitialize(this HubConnection hubConnection, NavigationManager navigationManager, ILocalStorageService _localStorage)
        {
            if (hubConnection == null)
            {
                var signalrEndpoint = $"https://localhost:7262{ApplicationConstants.SignalR.HubUrl}";
                var uri = new Uri(signalrEndpoint);
                var absoluteUrl = navigationManager
                                  .ToAbsoluteUri(ApplicationConstants.SignalR.HubUrl);

                hubConnection = new HubConnectionBuilder()
                                  .WithUrl(uri, options =>
                                  {
                                      options.AccessTokenProvider = async () => await _localStorage.GetItemAsync<string>("authToken");
                                  })
                                  .WithAutomaticReconnect()
                                  .Build();
            }
            return hubConnection;
        }
    }
}