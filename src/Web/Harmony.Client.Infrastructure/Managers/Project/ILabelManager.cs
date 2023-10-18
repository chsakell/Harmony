using Harmony.Application.Features.Labels.Commands.UpdateTitle;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface ILabelManager : IManager
    {
        Task<IResult> UpdateLabelTitle(UpdateLabelTitleCommand request);
    }
}
