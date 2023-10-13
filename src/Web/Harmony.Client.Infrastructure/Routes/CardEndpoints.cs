namespace Harmony.Client.Infrastructure.Routes
{
    public static class BoardEndpoints
    {
        public static string Index = "api/board/";

        public static string Get(string boardId)
        {
            return $"{Index}{boardId}";
        }

        public static string GetMembers(string boardId)
        {
            return $"{Index}{boardId}/members/";
        }

        public static string SearchMembers(string boardId, string term)
        {
            return $"{Index}{boardId}/members/search?term={term}";
        }

        public static string CreateCard(Guid boardId, Guid listId)
		{
			return $"{Index}{boardId}/lists/{listId}/cards/";
		}
    }
}