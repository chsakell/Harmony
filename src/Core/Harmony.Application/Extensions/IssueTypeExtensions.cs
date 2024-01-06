using Harmony.Application.DTO;
using Harmony.Shared.Constants.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
