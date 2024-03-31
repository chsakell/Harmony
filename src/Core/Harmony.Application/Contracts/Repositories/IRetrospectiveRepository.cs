using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    /// <summary>
    /// Repository to access Retrospectives
    /// </summary>
    public interface IRetrospectiveRepository
    {
        IQueryable<Retrospective> Entities { get; }
        Task<Retrospective?> GetRetrospective(Guid id);
        Task Add(Retrospective retrospective);
        Task<int> Create(Retrospective retrospective);
        Task<int> Update(Retrospective retrospective);
        Task<int> Delete(Retrospective retrospective);
        Task<int> CountRetrospectives(Guid boardId);
    }
}
