namespace Harmony.Client.Infrastructure.Routes
{
    public static class CheckListEndpoints
    {
        public static string Index = "api/checklists";

        public static string GetList(Guid checkListId) => $"{Index}/{checkListId}/";

        public static string GetListItems(Guid checkListId) => $"{Index}/{checkListId}/items/";
    }
}