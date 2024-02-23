using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class WorkspaceEndpoints
    {
        public static string Index = $"{GatewayConstants.CoreApiPrefix}/workspace/";

        public static string Get(string workspaceId)
        {
            return $"{Index}{workspaceId}";
        }

        public static string GetBoards(string workspaceId)
        {
            return $"{Get(workspaceId)}/boards/";
        }

        public static string GetMembers(string workspaceId)
        {
            return $"{Get(workspaceId)}/members/";
        }

        public static string SearchMembers(string workspaceId, string term)
        {
            return $"{Get(workspaceId)}/members/search/?term={term}";
        }

        public static string GetAddMembers(string workspaceId)
        {
            return $"{Get(workspaceId)}/members/add/";
        }

        public static string GetRemoveMembers(string workspaceId)
        {
            return $"{Get(workspaceId)}/members/remove/";
        }

        public static string GetMembers(Guid workspaceId, int pageNumber, int pageSize, string searchTerm, string[] orderBy, bool membersOnly)
        {
            var url = $"{Get(workspaceId.ToString())}/members/?pageNumber={pageNumber}&pageSize={pageSize}&membersOnly={membersOnly}&searchTerm={searchTerm}&orderBy=";
            if (orderBy?.Any() == true)
            {
                foreach (var orderByPart in orderBy)
                {
                    url += $"{orderByPart},";
                }
                url = url[..^1];
            }
            return url;
        }
    }
}