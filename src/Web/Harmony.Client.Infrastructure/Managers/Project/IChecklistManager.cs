using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Commands.CreateCard;
using Harmony.Application.Features.Cards.Commands.CreateChecklist;
using Harmony.Application.Features.Cards.Commands.CreateCheckListItem;
using Harmony.Application.Features.Cards.Commands.MoveCard;
using Harmony.Application.Features.Cards.Commands.UpdateCardDescription;
using Harmony.Application.Features.Cards.Queries.LoadCard;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IChecklistManager : IManager
    {
        Task<IResult<CheckListDto>> CreateCheckListAsync(CreateChecklistCommand request);
        Task<IResult<CheckListItemDto>> CreateCheckListItemAsync(CreateCheckListItemCommand request);
    }
}
