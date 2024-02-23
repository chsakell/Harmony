using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class AccountEndpoints
    {
        public static string Register = $"{GatewayConstants.CoreApiPrefix}/identity/account/register";
        public static string ChangePassword = $"{GatewayConstants.CoreApiPrefix}/identity/account/changepassword";
        public static string UpdateProfile = $"{GatewayConstants.CoreApiPrefix}/identity/account/updateprofile";

        public static string GetProfilePicture(string userId)
        {
            return $"{GatewayConstants.CoreApiPrefix}/identity/account/profile-picture/{userId}";
        }

        public static string UpdateProfilePicture(string userId)
        {
            return $"{GatewayConstants.CoreApiPrefix}/identity/account/profile-picture/{userId}";
        }
    }
}