using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Boards.Queries.GetUserBoards
{
    /// <summary>
    /// Query for getting boards a user has access to
    /// </summary>
    public class GetUserBoardsQuery : IRequest<Result<List<BoardDto>>>
    {
    }
}