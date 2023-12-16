using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Lists.Commands.UpdateListItemDueDate;

public class UpdateListItemDueDateCommand : BaseBoardCommand, IRequest<Result<bool>>
{
    public Guid ListItemId { get; set; }
    public DateTime? DueDate { get; set; }

    public UpdateListItemDueDateCommand(Guid listItemId, DateTime? dueDate)
    {
        ListItemId = listItemId;
        DueDate = dueDate;
    }
}
