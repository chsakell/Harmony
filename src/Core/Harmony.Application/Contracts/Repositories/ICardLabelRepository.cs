using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    /// <summary>
    /// Repository to access Card Labels
    /// </summary>
    public interface ICardLabelRepository
    {
        Task<CardLabel> GetLabel(Guid cardId, Guid labelId);
        Task<List<CardLabel>> GetLabels(Guid cardId);
        Task AddAsync(Label label);
        Task<int> CreateAsync(Label label);
        Task<Label> GetLabel(Guid labelId);
        Task<int> CreateCardLabelAsync(CardLabel label);
        Task<int> DeleteCardLabel(CardLabel label);
    }
}
