using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Boards.Commands.CreateList;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Lists.Commands.ArchiveList;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IBoardListManager : IManager
    {
		Task<IResult<BoardListDto>> CreateListAsync(CreateListCommand request);
        Task<IResult<bool>> UpdateListStatusAsync(UpdateListStatusCommand request);
    }
}
