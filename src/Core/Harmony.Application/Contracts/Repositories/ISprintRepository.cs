using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

namespace Harmony.Application.Contracts.Repositories
{
    /// <summary>
    /// Repository to access sprints
    /// </summary>
    public interface ISprintRepository
    {
        IQueryable<Sprint> Entities { get; }
        Task<Sprint?> GetSprint(Guid sprintId);
        Task<List<Sprint>> GetSprints(Guid boardId);
        Task AddAsync(Sprint sprint);
        Task<int> CreateAsync(Sprint sprint);
        Task<int> Update(Sprint sprint);
        Task<int> Delete(Sprint sprint);
        Task<int> CountSprints(Guid boardId);
        Task<List<Sprint>> SearchSprints(Guid boardId, string term, int pageNumber, int pageSize,
            List<SprintStatus> statuses);
        Task<List<Sprint>> GetActiveSprints(Guid boardId);
        Task<List<Sprint>> GetNonCompletedSprints(Guid boardId);
    }
}
