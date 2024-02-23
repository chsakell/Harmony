using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class CheckListEndpoints
    {
        public static string Index = $"{GatewayConstants.CoreApiPrefix}/checklists";

        public static string GetList(Guid checkListId) => $"{Index}/{checkListId}/";

        public static string GetListItems(Guid checkListId) => $"{Index}/{checkListId}/items/";

        public static string Title(Guid checkListId) => $"{Index}/{checkListId}/title/";
    }
}