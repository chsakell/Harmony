using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class UserNotificationsEndpoints
    {
        /// <summary>GET reads, and POST sets, the logged-in user's own notification settings.</summary>
        public static string Index = $"{GatewayConstants.CoreApiPrefix}/usernotifications";
    }
}