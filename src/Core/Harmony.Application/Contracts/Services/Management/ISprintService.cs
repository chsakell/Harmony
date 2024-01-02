using Harmony.Application.Features.Sprints.Queries.GetSprintReports;

namespace Harmony.Application.Contracts.Services.Management
{
    public interface ISprintService
    {
        Task<GetSprintReportsResponse> GetSprintReports(Guid sprintId);
    }
}
