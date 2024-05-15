using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.DeleteChecklist;

public class DeleteCheckListCommand : BaseBoardCommand, IRequest<Result<bool>>
{
    public Guid CheckListId { get; set; }

    public DeleteCheckListCommand(Guid checkListId)
    {
        CheckListId = checkListId;
    }
}
