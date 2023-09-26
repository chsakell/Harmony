using Harmony.Application.Features.Lists.Commands.UpdateListItemDescription;
using Harmony.Application.Features.Lists.Commands.UpdateListTitle;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface ICheckListItemManager : IManager
    {
        Task<IResult<bool>> UpdateListItemDescriptionAsync(UpdateListItemDescriptionCommand request);
    }
}
