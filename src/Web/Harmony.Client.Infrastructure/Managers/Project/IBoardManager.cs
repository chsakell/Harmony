using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IBoardManager : IManager
    {
        Task<IResult<Guid>> CreateAsync(CreateBoardCommand request);
        Task<IResult<GetBoardResponse>> GetBoardAsync(string boardId);
        event EventHandler<BoardCreatedEvent> OnBoardCreated;
    }
}
