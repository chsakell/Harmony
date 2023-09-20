namespace Harmony.Client.Infrastructure.Routes
{
    public static class BoardEndpoints
    {
        public static string Index = "api/board/";

        public static string Get(string boardId)
        {
            return $"{Index}{boardId}";
        }

		public static string CreateList(Guid boardId)
		{
			return $"{Index}{boardId}/lists/";
		}

		public static string CreateCard(Guid boardId, Guid listId)
		{
			return $"{Index}{boardId}/lists/{listId}/cards/";
		}
	}
}