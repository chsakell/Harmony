using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Labels.Commands.UpdateTitle;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface ILabelManager : IManager
    {
        Task<IResult> UpdateLabelTitle(UpdateLabelTitleCommand request);
    }
}
