using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Lists.Commands.UpdateListItemChecked;

public class UpdateListItemCheckedCommand : BaseBoardCommand, IRequest<Result<bool>>
{
    public Guid CardId { get; set; }
    public Guid ListItemId { get; set; }
    public bool IsChecked { get; set; }

    public UpdateListItemCheckedCommand(Guid listItemId, bool isChecked, Guid cardId)
    {
        ListItemId = listItemId;
        IsChecked = isChecked;
        CardId = cardId;
    }
}
