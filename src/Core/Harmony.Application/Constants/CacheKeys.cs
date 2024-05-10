namespace Harmony.Application.Constants
{
    public class CacheKeys
    {
        public static string BoardInfo(Guid boardId) => $"board-info-{boardId}";
        public static string BoardSummary(Guid boardId) => $"board-summary-{boardId}";
        public static string BoardMembers(Guid boardId) => $"board-members-{boardId}";
        public static string BoardMemberAccess(Guid boardId, string userId) => $"board-member-access-{boardId}-{userId}";
        public static string CardSummary(Guid cardId) => $"card-summary-{cardId}";
        public static string BoardIdFromCard(Guid cardId) => $"boardid-from-card-{cardId}";
        public static string BoardIssueTypes(Guid boardId) => $"board-issue-types-{boardId}";
        public const string AutomationTemplates = "automation-templates";
    }
}
