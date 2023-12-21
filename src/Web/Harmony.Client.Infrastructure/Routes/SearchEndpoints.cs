namespace Harmony.Client.Infrastructure.Routes
{
    public static class SearchEndpoints
    {
        public static string Index = "api/search/";

        public static string Search(string text)
        {
            return $"{Index}?text={text}";
        }
    }
}