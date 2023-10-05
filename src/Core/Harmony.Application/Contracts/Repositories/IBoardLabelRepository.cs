using Harmony.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Repositories
{
    public interface IBoardLabelRepository
    {
        Task<Label?> GetLabel(Guid labelId);
        Task<List<Label>> GetLabels(Guid boardId);
        Task AddAsync(Label label);
        Task<int> CreateAsync(Label label);
        Task<int> Update(Label label);
    }
}
