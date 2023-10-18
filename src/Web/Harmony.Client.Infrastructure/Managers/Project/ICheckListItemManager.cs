using Harmony.Application.Features.Lists.Commands.UpdateListItemChecked;
using Harmony.Application.Features.Lists.Commands.UpdateListItemDescription;
using Harmony.Application.Features.Lists.Commands.UpdateListItemDueDate;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface ICheckListItemManager : IManager
    {
        Task<IResult<bool>> UpdateListItemDescriptionAsync(UpdateListItemDescriptionCommand request);
        Task<IResult<bool>> UpdateListItemCheckedAsync(UpdateListItemCheckedCommand request);
        Task<IResult<bool>> UpdateListItemDueDateAsync(UpdateListItemDueDateCommand request);
    }
}
