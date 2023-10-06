using Harmony.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Repositories
{
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
