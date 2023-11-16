namespace Harmony.Client.Infrastructure.Routes
{
    public static class CardEndpoints
    {
        public static string Index = "api/cards";

        public static string Get(int id) => $"api/cards/{id}/";
        public static string GetLabels(int id) => $"api/cards/{id}/labels/";
        public static string GetActivity(int id) => $"api/cards/{id}/activity/";
        public static string Move(int cardId) => $"{Index}/{cardId}/move/";

        public static string Description(int cardId) => $"{Index}/{cardId}/description/";
        public static string Title(int cardId) => $"{Index}/{cardId}/title/";
        public static string Checklists(int cardId) => $"{Index}/{cardId}/checklists/";
        public static string Status(int cardId) => $"{Index}/{cardId}/status/";
        public static string Labels(int cardId) => $"{Index}/{cardId}/labels/";
        public static string Dates(int cardId) => $"{Index}/{cardId}/dates/";
        public static string GetMembers(string cardId)
        {
            return $"{Index}/{cardId}/members/";
        }

        public static string GetCardMember(string cardId, string userId)
        {
            return $"{Index}/{cardId}/members/{userId}/";
        }

        public static string GetCardAttachment(int cardId, Guid attachmentId)
        {
            return $"{Index}/{cardId}/attachments/{attachmentId}/";
        }
    }
}