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
        Task AddAsync(Label label);
        Task<int> CreateAsync(Label label);
    }
}
