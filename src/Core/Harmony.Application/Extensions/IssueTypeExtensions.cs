using Harmony.Application.DTO;
using Harmony.Shared.Constants.Application;

namespace Harmony.Application.Extensions
{
    public static class IssueTypeExtensions
    {
        public static List<IssueTypeDto> Normal(this List<IssueTypeDto> issues)
        {
            if (issues == null)
            {
                return issues;
            }

            return issues.Where(i => !i.Summary.Contains(IssueTypesConstants.SUBTASK)).ToList();
        }
    }
}
