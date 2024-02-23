using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class FileEndpoints
    {
        public static string Index = $"{GatewayConstants.CoreApiPrefix}/files";

        public static string ProfilePicture = $"{Index}/profile-picture/";
    }
}