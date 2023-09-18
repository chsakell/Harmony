namespace Harmony.Client.Infrastructure.Routes
{
    public static class WorkspaceEndpoints
    {
        public static string Index = "api/workspace/";

        public static string Get(string workspaceId)
        {
            return $"{Index}{workspaceId}";
        }
    }
}