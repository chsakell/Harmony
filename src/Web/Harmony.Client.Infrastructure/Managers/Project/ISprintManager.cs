using Harmony.Application.Features.Sprints.Commands.StartSprint;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface ISprintManager : IManager
    {
        Task<IResult> StartSprint(StartSprintCommand request);
    }
}
