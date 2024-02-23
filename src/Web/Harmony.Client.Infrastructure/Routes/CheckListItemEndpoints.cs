using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class CheckListItemEndpoints
    {
        public static string Index = $"{GatewayConstants.CoreApiPrefix}/checklistitems";

        public static string GetListItem(Guid checkListItemId) => $"{Index}/{checkListItemId}/";

        public static string Description(Guid checkListItemId) => $"{Index}/{checkListItemId}/description/";

        public static string Checked(Guid checkListItemId) => $"{Index}/{checkListItemId}/checked/";

        public static string DueDate(Guid checkListItemId) => $"{Index}/{checkListItemId}/duedate/";
    }
}