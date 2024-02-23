using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class TokenEndpoints
    {
        public static string Get = $"{GatewayConstants.CoreApiPrefix}/identity/token";
        public static string Refresh = $"{GatewayConstants.CoreApiPrefix}/identity/token/refresh";
    }
}