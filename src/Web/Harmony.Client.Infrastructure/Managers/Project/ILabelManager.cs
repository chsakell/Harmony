using Harmony.Application.Features.Labels.Commands.CreateCardLabel;
using Harmony.Application.Features.Labels.Commands.RemoveCardLabel;
using Harmony.Application.Features.Labels.Commands.UpdateTitle;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface ILabelManager : IManager
    {
        Task<IResult<CreateCardLabelResponse>> CreateCardLabel(CreateCardLabelCommand request);
        Task<IResult> UpdateLabelTitle(UpdateLabelTitleCommand request);
        Task<IResult<bool>> RemoveCardLabel(RemoveCardLabelCommand request);
    }
}
