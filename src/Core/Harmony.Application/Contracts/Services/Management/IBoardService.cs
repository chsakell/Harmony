using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Services.Management
{
    public interface IBoardService
	{
		Task<Board> LoadBoard(Guid boardId, int maxCardsPerList);
	}
}
