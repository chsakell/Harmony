using Harmony.Application.Features.Sprints.Queries.GetSprintReports;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

namespace Harmony.Application.Contracts.Services.Management
{
    public interface ISprintService
    {
        Task<GetSprintReportsResponse> GetSprintReports(Guid sprintId);
        Task<List<Card>> GetSprintsCards(Guid sprintId, string term,
            int pageNumber, int pageSize, CardStatus? status);
    }
}
