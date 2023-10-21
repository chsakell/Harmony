namespace Harmony.Client.Infrastructure.Routes
{
    public static class BoardListEndpoints
    {
        public static string Index = "api/boardlists";

        public static string GetList(Guid listId) => $"{Index}/{listId}/";

        public static string GetListTitle(Guid listId) => $"{Index}/{listId}/title";
    }
}