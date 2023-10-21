using Harmony.Application.DTO;
using Harmony.Application.Features.Lists.Commands.ArchiveList;
using Harmony.Application.Features.Lists.Commands.CreateList;
using Harmony.Application.Features.Lists.Commands.UpdateListTitle;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IBoardListManager : IManager
    {
		Task<IResult<BoardListDto>> CreateListAsync(CreateListCommand request);
        Task<IResult<UpdateListTitleResponse>> UpdateBoardListTitleAsync(UpdateListTitleCommand request);

        Task<IResult<bool>> UpdateListStatusAsync(UpdateListStatusCommand request);
    }
}
