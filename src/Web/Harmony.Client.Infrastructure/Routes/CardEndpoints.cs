using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class CardEndpoints
    {
        public static string Index = $"{GatewayConstants.CoreApiPrefix}/cards";

        public static string Backlog => $"{GatewayConstants.CoreApiPrefix}/cards/backlog/";
        public static string BacklogItem(Guid cardId) => $"{GatewayConstants.CoreApiPrefix}/cards/backlog/{cardId}/";
        public static string Get(Guid id) => $"{GatewayConstants.CoreApiPrefix}/cards/{id}/";

        public static string ChildIssue(Guid id) => $"{GatewayConstants.CoreApiPrefix}/cards/{id}/childissue";
        public static string GetLabels(Guid id) => $"{GatewayConstants.CoreApiPrefix}/cards/{id}/labels/";
        public static string GetActivity(Guid id) => $"{GatewayConstants.CoreApiPrefix}/cards/{id}/activity/";
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