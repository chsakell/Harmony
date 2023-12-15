using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Features.Sprints.Queries.GetSprintReports;
using Harmony.Domain.Entities;
using static Harmony.Application.Events.BoardListArchivedEvent;

namespace Harmony.Infrastructure.Services.Management
{
    public class SprintService : ISprintService
    {
        private readonly ISprintRepository _sprintRepository;

        public SprintService(ISprintRepository sprintRepository)
        {
            _sprintRepository = sprintRepository;
        }

        public async Task<GetSprintReportsResponse> GetSprintReports(Guid boardId, Guid sprintId)
        {
            var sprint = await _sprintRepository.GetSprints(sprintId);



            throw new NotImplementedException();
        }
    }
}
