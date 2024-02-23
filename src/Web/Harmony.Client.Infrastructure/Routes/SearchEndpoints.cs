using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class SearchEndpoints
    {
        public static string Index = $"{GatewayConstants.CoreApiPrefix}/search/";

        public static string Search(string text)
        {
            return $"{Index}?text={text}";
        }

        public static string AdvancedSearch = $"{Index}advanced";
    }
}