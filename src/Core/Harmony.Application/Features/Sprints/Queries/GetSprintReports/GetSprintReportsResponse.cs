using Harmony.Application.DTO;

namespace Harmony.Application.Features.Sprints.Queries.GetSprintReports
{
    public class GetSprintReportsResponse
    {
        public int TotalStoryPoints { get; set; }
        public int RemainingStoryPoints { get; set; }
        public SprintDto Sprint { get; set; }
        public BurnDownReportDto BurnDownReport { get; set; }
        public IssuesOverviewReportDto IssuesOverviewReport { get; set; }
    }
}
