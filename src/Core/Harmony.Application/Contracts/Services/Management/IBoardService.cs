using Harmony.Application.Features.Boards.Queries.GetSprintsSummary;
using Harmony.Application.Models;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

namespace Harmony.Application.Contracts.Services.Management
{
    /// <summary>
    /// Service board related operations
    /// </summary>
    public interface IBoardService
	{
        Task<bool> HasUserAccessToBoard(string userId, Guid boardId);
        Task<List<Board>> GetUserBoards(Guid? workspaceId, string userId);
        Task<List<Board>> GetUserBoardsWithLists(Guid? workspaceId, string userId);
        Task<Board> LoadBoardOld(Guid boardId, int maxCardsPerList, Guid? sprintId = null);
        Task<Board> LoadBoard(Guid boardId, int maxCardsPerList, Guid? sprintId = null);
        Task<List<Card>> LoadBoardListCards(Guid boardId, Guid boardListId, int page, int maxCardsPerList, Guid? sprintId = null);
        Task<List<SprintSummary>> GetSprintsSummaries(Guid boardId, string term, int pageNumber, int pageSize, SprintStatus? status);
        Task<BoardInfo?> GetBoardInfo(Guid boardId);
        Task<List<Board>> GetStatusForBoards(List<Guid> boardIds);
    }
}
