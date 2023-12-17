namespace Harmony.Client.Infrastructure.Routes
{
    public static class CardEndpoints
    {
        public static string Index = "api/cards";

        public static string Backlog => $"api/cards/backlog/";
        public static string BacklogItem(Guid cardId) => $"api/cards/backlog/{cardId}/";
        public static string Get(Guid id) => $"api/cards/{id}/";
        public static string GetLabels(Guid id) => $"api/cards/{id}/labels/";
        public static string GetActivity(Guid id) => $"api/cards/{id}/activity/";
        public static string Move(Guid cardId) => $"{Index}/{cardId}/move/";

        public static string Description(Guid cardId) => $"{Index}/{cardId}/description/";
        public static string StoryPoints(Guid cardId) => $"{Index}/{cardId}/storypoints/";
        public static string IssueType(Guid cardId) => $"{Index}/{cardId}/issuetype/";
        public static string Title(Guid cardId) => $"{Index}/{cardId}/title/";
        public static string Checklists(Guid cardId) => $"{Index}/{cardId}/checklists/";
        public static string Status(Guid cardId) => $"{Index}/{cardId}/status/";
        public static string Labels(Guid cardId) => $"{Index}/{cardId}/labels/";
        public static string Dates(Guid cardId) => $"{Index}/{cardId}/dates/";
        public static string GetMembers(string cardId)
        {
            return $"{Index}/{cardId}/members/";
        }

        public static string GetCardMember(string cardId, string userId, Guid boardId)
        {
            return $"{Index}/{cardId}/members/{userId}/?boardId={boardId}";
        }

        public static string GetCardAttachment(Guid cardId, Guid attachmentId)
        {
            return $"{Index}/{cardId}/attachments/{attachmentId}/";
        }
    }
}