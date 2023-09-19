namespace Harmony.Client.Infrastructure.Routes
{
    public static class BoardEndpoints
    {
        public static string Index = "api/board/";

        public static string Get(string boardId)
        {
            return $"{Index}{boardId}";
        }
    }
}