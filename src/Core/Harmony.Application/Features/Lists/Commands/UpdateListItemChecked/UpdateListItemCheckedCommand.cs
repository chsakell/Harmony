using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Lists.Commands.UpdateListItemChecked;

public class UpdateListItemCheckedCommand : IRequest<Result<bool>>
{
    public int CardId { get; set; }
    public Guid ListItemId { get; set; }
    public bool IsChecked { get; set; }

    public UpdateListItemCheckedCommand(Guid listItemId, bool isChecked, int cardId)
    {
        ListItemId = listItemId;
        IsChecked = isChecked;
        CardId = cardId;
    }
}
