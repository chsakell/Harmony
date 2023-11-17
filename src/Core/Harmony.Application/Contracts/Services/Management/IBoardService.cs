using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Services.Management
{
    /// <summary>
    /// Service board related operations
    /// </summary>
    public interface IBoardService
	{
        Task<bool> HasUserAccessToBoard(string userId, Guid boardId);
        Task<List<Board>> GetUserBoards(Guid workspaceId, string userId);
        Task<Board> LoadBoard(Guid boardId, int maxCardsPerList);
        Task<List<Card>> LoadBoardListCards(Guid boardId, Guid boardListId, int page, int maxCardsPerList);
        Task<List<GetSprintItemResponse>> SearchSprints(Guid boardId, string term, int pageNumber, int pageSize);
    }
}
