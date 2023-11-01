using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Commands.CreateChecklist;
using Harmony.Application.Features.Cards.Commands.CreateCheckListItem;
using Harmony.Application.Features.Cards.Commands.DeleteChecklist;
using Harmony.Application.Features.Lists.Commands.UpdateCheckListTitle;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface ICheckListManager : IManager
    {
        Task<IResult<CheckListDto>> CreateCheckListAsync(CreateCheckListCommand request);
        Task<IResult<CheckListItemDto>> CreateCheckListItemAsync(CreateCheckListItemCommand request);
        Task<IResult<bool>> UpdateTitleAsync(UpdateCheckListTitleCommand request);
        Task<IResult<bool>> DeleteCheckListAsync(DeleteCheckListCommand request);
    }
}
