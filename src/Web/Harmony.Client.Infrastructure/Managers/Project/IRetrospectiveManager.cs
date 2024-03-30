
using Harmony.Application.DTO;
using Harmony.Application.Features.Retrospectives.Commands.Create;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IRetrospectiveManager : IManager
    {
        Task<IResult<RetrospectiveDto>> Create(CreateRetrospectiveCommand command);
    }
}
