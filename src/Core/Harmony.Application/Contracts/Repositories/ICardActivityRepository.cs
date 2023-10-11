using Harmony.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Repositories
{
    public interface ICardActivityRepository
    {
        Task<List<CardActivity>> GetAsync(Guid cardId);
        Task<int> CreateAsync(CardActivity activity);
    }
}
