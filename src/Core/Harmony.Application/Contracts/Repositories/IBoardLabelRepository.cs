using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    /// <summary>
    /// Repository to access Labels
    /// </summary>
    public interface IBoardLabelRepository
    {
        Task<Label?> GetLabel(Guid labelId);
        Task<List<Label>> GetLabels(Guid boardId);
        Task AddAsync(Label label);
        Task<int> CreateAsync(Label label);
        Task<int> Update(Label label);
        Task<int> Delete(Label label);
    }
}
