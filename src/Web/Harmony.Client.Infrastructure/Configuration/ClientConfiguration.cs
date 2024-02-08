namespace Harmony.Client.Infrastructure.Configuration
{
    public class ClientConfiguration
    {
        public ClientConfiguration(string signalrHostUrl, string backendUrl)
        {
            SignalrHostUrl = signalrHostUrl;
            BackendUrl = backendUrl;
        }

        public string SignalrHostUrl {  get; private set; }
        public string BackendUrl { get; private set;}

        public string GetServerResource(string resource)
        {
            return $"{BackendUrl.TrimEnd('/')}/{resource}";
        }
    }
}
