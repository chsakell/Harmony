namespace Harmony.Client.Infrastructure.Configuration
{
    public class ClientConfiguration
    {
        public ClientConfiguration(string gatewayUrl)
        {
            GatewayUrl = gatewayUrl;
        }

        public string GatewayUrl { get; }

        public string GetServerResource(string resource)
        {
            return $"{GatewayUrl.TrimEnd('/')}/core/{resource}";
        }
    }
}
