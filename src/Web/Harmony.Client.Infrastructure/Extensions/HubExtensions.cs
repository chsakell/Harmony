using Microsoft.AspNetCore.Components;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Harmony.Shared.Constants.Application;

namespace Harmony.Client.Infrastructure.Extensions
{
    public static class HubExtensions
    {
        public static HubConnection Get(this HubConnection hubConnection, 
            NavigationManager navigationManager, ILocalStorageService _localStorage, string signalrHostUrl)
        {
            if (hubConnection == null)
            {
                var host = signalrHostUrl.TrimEnd('/');
                var signalrEndpoint = $"{host}{ApplicationConstants.SignalR.HubUrl}";
                var uri = new Uri(signalrEndpoint);
                var absoluteUrl = navigationManager
                                  .ToAbsoluteUri(ApplicationConstants.SignalR.HubUrl);

                hubConnection = new HubConnectionBuilder()
                                  .WithUrl(uri, options =>
                                  {
                                      options.SkipNegotiation = true;
                                      options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                                      options.AccessTokenProvider = async () => await _localStorage.GetItemAsync<string>("authToken");
                                  })
                                  .WithAutomaticReconnect()
                                  .Build();
            }
            return hubConnection;
        }
    }
}