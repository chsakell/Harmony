using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Services.Management
{
    public interface IBoardService
	{
        Task<bool> HasUserAccessToBoard(string userId, Guid boardId);

        Task<Board> LoadBoard(Guid boardId, int maxCardsPerList);
	}
}
