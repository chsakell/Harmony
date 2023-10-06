namespace Harmony.Client.Infrastructure.Routes
{
    public static class CardEndpoints
    {
        public static string Index = "api/cards";

        public static string Get(Guid id) => $"api/cards/{id}/";
        public static string GetLabels(Guid id) => $"api/cards/{id}/labels/";

        public static string Move(Guid cardId) => $"{Index}/{cardId}/move/";

        public static string Description(Guid cardId) => $"{Index}/{cardId}/description/";
        public static string Title(Guid cardId) => $"{Index}/{cardId}/title/";
        public static string Checklists(Guid cardId) => $"{Index}/{cardId}/checklists/";
        public static string Status(Guid cardId) => $"{Index}/{cardId}/status/";
        public static string Labels(Guid cardId) => $"{Index}/{cardId}/labels/";
    }
}