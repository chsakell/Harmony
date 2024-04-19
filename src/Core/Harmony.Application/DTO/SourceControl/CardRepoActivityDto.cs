
using Harmony.Domain.Enums.SourceControl;

namespace Harmony.Application.DTO.SourceControl
{
    public class CardRepoActivityDto
    {
        public SourceControlProvider Provider { get; set; }
        public int TotalBranches { get; set; }
        public int TotalPushed { get; set; }
        public int TotalPullRequests { get; set; }
    }
}
