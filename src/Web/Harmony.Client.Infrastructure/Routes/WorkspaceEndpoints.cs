namespace Harmony.Client.Infrastructure.Routes
{
    public static class WorkspaceEndpoints
    {
        public static string Index = "api/workspace/";

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
    }
}