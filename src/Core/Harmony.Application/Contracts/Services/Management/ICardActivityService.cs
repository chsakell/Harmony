using Harmony.Application.DTO;
using Harmony.Domain.Enums;

namespace Harmony.Application.Contracts.Services.Management
{
    /// <summary>
    /// Service to access Card Activities
    /// </summary>
    public interface ICardActivityService
    {
        Task<List<CardActivityDto>> GetAsync(Guid cardId);
        Task CreateActivity(Guid cardId, string userId, CardActivityType type, 
            DateTime date, string extraInfo = null, string url = null);
        Task<List<BoardActivityDto>> GetBoardsActivities(List<Guid> boardIds);
    }
}
