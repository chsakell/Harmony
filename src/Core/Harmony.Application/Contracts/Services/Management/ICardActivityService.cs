using Harmony.Application.DTO;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Services.Management
{
    public interface ICardActivityService
    {
        Task<List<CardActivityDto>> GetAsync(Guid cardId);
        Task CreateActivity(Guid cardId, string userId, CardActivityType type, 
            DateTime date, string extraInfo = null, string url = null);
    }
}
