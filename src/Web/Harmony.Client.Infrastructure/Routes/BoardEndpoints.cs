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

        public static string Member(string boardId, string userId)
        {
            return $"{Index}{boardId}/members/{userId}/";
        }

        public static string MemberStatus(string boardId, string userId)
        {
            return $"{Index}{boardId}/members/{userId}/status/";
        }

        public static string SearchMembers(string boardId, string term)
        {
            return $"{Index}{boardId}/members/search?term={term}";
        }

        public static string CreateCard(Guid boardId, Guid listId)
		{
			return $"{Index}{boardId}/lists/{listId}/cards/";
		}

        public static string BoardListPositions(string boardId)
        {
            return $"{Index}{boardId}/positions/";
        }

        public static string GetBoardList(string boardId, Guid listId, int page, int maxCards)
        {
            return $"{Index}{boardId}/lists/{listId}/?page={page}&maxCards={maxCards}";
        }
    }
}