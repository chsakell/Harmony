namespace Harmony.Client.Infrastructure.Configuration
{
    public class ClientConfiguration
    {
        public ClientConfiguration(string signalrHostUrl, string backendUrl, string gatewayUrl)
        {
            SignalrHostUrl = signalrHostUrl;
            BackendUrl = backendUrl;
            GatewayUrl = gatewayUrl;
        }

        public string SignalrHostUrl {  get; private set; }
        public string BackendUrl { get; private set;}
        public string GatewayUrl { get; }

        public string GetServerResource(string resource)
        {
            return $"{BackendUrl.TrimEnd('/')}/{resource}";
        }
    }
}
