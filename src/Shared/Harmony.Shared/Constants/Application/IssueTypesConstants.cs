namespace Harmony.Shared.Constants.Application
{
    public class IssueTypesConstants
    {
        public const string EPIC = "Epic";
        public const string BUG = "Bug";
        public const string STORY = "Story";
        public const string TASK = "Task";
        public const string SUBTASK = "SubTask";

        public static List<string> GetDefaultIssueTypes()
        {
            return new List<string> { TASK, STORY, BUG, EPIC, SUBTASK };
        }
    }
}
