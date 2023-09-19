using Harmony.Application.Features.Boards.Commands;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IBoardManager : IManager
    {
        Task<IResult> CreateAsync(CreateBoardCommand request);
    }
}
