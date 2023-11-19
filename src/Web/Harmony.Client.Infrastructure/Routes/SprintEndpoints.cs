namespace Harmony.Client.Infrastructure.Routes
{
    public static class SprintEndpoints
    {
        public static string Index = "api/sprints";

        public static string Start(Guid sprintId) => $"{Index}/{sprintId}/start/";
    }
}