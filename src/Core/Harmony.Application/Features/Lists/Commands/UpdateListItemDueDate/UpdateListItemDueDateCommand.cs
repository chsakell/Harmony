using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Lists.Commands.UpdateListItemDueDate;

public class UpdateListItemDueDateCommand : IRequest<Result<bool>>
{
    public Guid ListItemId { get; set; }
    public DateTime? DueDate { get; set; }

    public UpdateListItemDueDateCommand(Guid listItemId, DateTime? dueDate)
    {
        ListItemId = listItemId;
        DueDate = dueDate;
    }
}
