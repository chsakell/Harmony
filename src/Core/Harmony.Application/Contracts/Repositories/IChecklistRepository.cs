using Harmony.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Application.Contracts.Repositories
{
    public interface IChecklistRepository
    {
        Task<int> Add(CheckList card);
        Task<CheckList?> Get(Guid checklistId);
        Task<int> Update(CheckList checklist);
    }
}
