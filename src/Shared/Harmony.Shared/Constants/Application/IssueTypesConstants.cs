namespace Harmony.Shared.Constants.Application
{
    public class IssueTypesConstants
    {
        #region Default

        public const string EPIC = "Epic";
        public const string BUG = "Bug";
        public const string STORY = "Story";
        public const string TASK = "Task";
        public const string SUBTASK = "SubTask";

        #endregion

        #region Retrospective

        public const string NEUTRAL = "Neutral";
        public const string LIKE = "Like";
        public const string LOVE = "Love";
        public const string SAD = "Sad";
        public const string ANGRY = "Angry";

        #endregion

        public static List<string> GetDefaultIssueTypes()
        {
            return new List<string> { TASK, STORY, BUG, EPIC, SUBTASK };
        }

        public static List<string> GetRetrospectiveIssueTypes()
        {
            return new List<string> { NEUTRAL, LIKE, LOVE, SAD, ANGRY };
        }
    }
}
