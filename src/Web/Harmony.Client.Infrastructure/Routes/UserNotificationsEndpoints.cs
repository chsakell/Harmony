using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class UserNotificationsEndpoints
    {
        public static string Index = $"{GatewayConstants.CoreApiPrefix}/usernotifications";

        public static string User(string userId) => $"{Index}/{userId}/";
    }
}