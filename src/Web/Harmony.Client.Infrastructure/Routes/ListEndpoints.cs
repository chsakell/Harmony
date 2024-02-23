using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class ListEndpoints
    {
        public static string Index = $"{GatewayConstants.CoreApiPrefix}/lists";

        public static string GetList(Guid listId) => $"{Index}/{listId}/";

        public static string GetListStatus(Guid listId) => $"{Index}/{listId}/status";
    }
}