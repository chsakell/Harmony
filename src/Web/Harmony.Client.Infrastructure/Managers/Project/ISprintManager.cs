using Harmony.Application.Features.Sprints.StartSprint;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface ISprintManager : IManager
    {
        Task<IResult> StartSprint(StartSprintCommand request);
    }
}
