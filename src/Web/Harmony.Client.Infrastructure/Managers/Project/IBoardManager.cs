using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Boards.Commands.CreateList;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetAllForUser;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IBoardManager : IManager
    {
        Task<IResult> CreateAsync(CreateBoardCommand request);
        Task<IResult<List<GetAllForUserBoardResponse>>> GetUserBoardsAsync();
        Task<IResult<GetBoardResponse>> GetBoardAsync(string boardId);
		Task<IResult<Guid>> CreateListAsync(CreateListCommand request);
	}
}
