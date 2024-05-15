using MediatR;

namespace Harmony.Application.Constants
{
    public class CacheKeys
    {
        public static string Board(Guid boardId) => $"board-{boardId}";
        public static string BoardMembers(Guid boardId) => $"board-members-{boardId}";
        public static string BoardLists(Guid boardId) => $"board-lists-{boardId}";
        public static string BoardLabels(Guid boardId) => $"board-labels-{boardId}";
        public static string BoardMemberAccess(Guid boardId, string userId) => $"board-member-access-{boardId}-{userId}";
        public static string ActiveCardSummaries(Guid boardId) => $"board-active-card-summaries-{boardId}";
        public static string BoardIdFromCard(Guid cardId) => $"boardid-from-card-{cardId}";
        public static string BoardIssueTypes(Guid boardId) => $"board-issue-types-{boardId}";
        public const string AutomationTemplates = "automation-templates";
    }
}
