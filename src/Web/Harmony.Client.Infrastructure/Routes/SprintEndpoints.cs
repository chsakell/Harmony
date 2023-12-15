namespace Harmony.Client.Infrastructure.Routes
{
    public static class SprintEndpoints
    {
        public static string Index = "api/sprints";

        public static string Reports(Guid sprintId) => $"{Index}/{sprintId}/reports/";

        public static string Start(Guid sprintId) => $"{Index}/{sprintId}/start/";

        public static string Complete(Guid sprintId) => $"{Index}/{sprintId}/complete/";
    }
}