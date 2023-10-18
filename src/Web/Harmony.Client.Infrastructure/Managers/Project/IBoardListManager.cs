using Harmony.Application.DTO;
using Harmony.Application.Features.Lists.Commands.ArchiveList;
using Harmony.Application.Features.Lists.Commands.CreateList;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IBoardListManager : IManager
    {
		Task<IResult<BoardListDto>> CreateListAsync(CreateListCommand request);
        Task<IResult<bool>> UpdateListStatusAsync(UpdateListStatusCommand request);
    }
}
