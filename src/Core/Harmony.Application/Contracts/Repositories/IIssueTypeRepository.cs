using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    /// <summary>
    /// Repository to access Issue Types
    /// </summary>
    public interface IIssueTypeRepository
    {
        IQueryable<IssueType> Entities { get; }
        Task<IssueType?> GetIssueType(Guid labelId);
        Task<List<IssueType>> GetIssueTypes(Guid boardId);
        Task AddAsync(IssueType label);
        Task<int> CreateAsync(IssueType label);
        Task<int> Update(IssueType label);
        Task<int> Delete(IssueType label);
    }
}
