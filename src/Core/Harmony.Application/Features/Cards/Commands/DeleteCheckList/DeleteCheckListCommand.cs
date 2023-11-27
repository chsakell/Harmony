using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.DeleteChecklist;

public class DeleteCheckListCommand : IRequest<Result<bool>>
{
    public Guid CheckListId { get; set; }

    public DeleteCheckListCommand(Guid checkListId)
    {
        CheckListId = checkListId;
    }
}
