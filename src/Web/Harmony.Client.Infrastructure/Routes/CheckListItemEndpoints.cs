namespace Harmony.Client.Infrastructure.Routes
{
    public static class CheckListItemEndpoints
    {
        public static string Index = "api/checklistitems";

        public static string GetListItem(Guid checkListItemId) => $"{Index}/{checkListItemId}/";

        public static string Description(Guid checkListItemId) => $"{Index}/{checkListItemId}/description/";
    }
}