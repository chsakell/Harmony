using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Lists.Commands.UpdateListItemDescription;

public class UpdateListItemDescriptionCommand : BaseBoardCommand, IRequest<Result<bool>>
{
    public Guid ListItemId { get; set; }
    public string Description { get; set; }

    public UpdateListItemDescriptionCommand(Guid listItemId, string description)
    {
        ListItemId = listItemId;
        Description = description;
    }
}
