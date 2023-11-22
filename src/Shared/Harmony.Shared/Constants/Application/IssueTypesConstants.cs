using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Shared.Constants.Application
{
    public class IssueTypesConstants
    {
        public const string EPIC = "Epic";
        public const string BUG = "Bug";
        public const string STORY = "Story";
        public const string TASK = "Task";

        public static List<string> GetDefaultIssueTypes()
        {
            return new List<string> { TASK, STORY, BUG, EPIC };
        }
    }
}
