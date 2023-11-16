using Harmony.Application.DTO;
using Harmony.Domain.Enums;

namespace Harmony.Application.Contracts.Services.Management
{
    /// <summary>
    /// Service to access Card Activities
    /// </summary>
    public interface ICardActivityService
    {
        Task<List<CardActivityDto>> GetAsync(int cardId);
        Task CreateActivity(int cardId, string userId, CardActivityType type, 
            DateTime date, string extraInfo = null, string url = null);
        Task<List<BoardActivityDto>> GetBoardsActivities(List<Guid> boardIds);
    }
}
