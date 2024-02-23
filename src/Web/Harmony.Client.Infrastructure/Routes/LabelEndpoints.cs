using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class LabelEndpoints
    {
        public static string Index = $"{GatewayConstants.CoreApiPrefix}/labels";

        public static string GetLabel(Guid labelId) => $"{Index}/{labelId}/";

        public static string LabelTitle(Guid labelId) => $"{Index}/{labelId}/title/";
    }
}