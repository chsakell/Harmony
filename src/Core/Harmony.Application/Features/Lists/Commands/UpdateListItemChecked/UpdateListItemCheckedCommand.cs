using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Lists.Commands.UpdateListItemChecked;

public class UpdateListItemCheckedCommand : IRequest<Result<bool>>
{
    public Guid ListItemId { get; set; }
    public bool IsChecked { get; set; }

    public UpdateListItemCheckedCommand(Guid listItemId, bool isChecked)
    {
        ListItemId = listItemId;
        IsChecked = isChecked;
    }
}
