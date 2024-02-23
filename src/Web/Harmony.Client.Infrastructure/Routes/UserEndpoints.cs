using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class UserEndpoints
    {
        public static string GetAll = $"{GatewayConstants.CoreApiPrefix}/identity/user";

        public static string Get(string userId)
        {
            return $"{GatewayConstants.CoreApiPrefix}/identity/user/{userId}";
        }

        public static string GetUserRoles(string userId)
        {
            return $"{GatewayConstants.CoreApiPrefix}/identity/user/roles/{userId}";
        }

        public static string ExportFiltered(string searchString)
        {
            return $"{Export}?searchString={searchString}";
        }

        public static string Export = $"{GatewayConstants.CoreApiPrefix}/identity/user/export";
        public static string Register = $"{GatewayConstants.CoreApiPrefix}/identity/user";
        public static string ToggleUserStatus = $"{GatewayConstants.CoreApiPrefix}/identity/user/toggle-status";
        public static string ForgotPassword = $"{GatewayConstants.CoreApiPrefix}/identity/user/forgot-password";
        public static string ResetPassword = $"{GatewayConstants.CoreApiPrefix}/identity/user/reset-password";
    }
}