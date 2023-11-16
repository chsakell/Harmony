using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.CreateCheckListItem;

public class CreateCheckListItemCommand : IRequest<Result<CheckListItemDto>>
{
    public string Description { get; set; }
    public Guid CheckListId { get; set; }
    public DateTime? DueDate { get; set; }
    public int CardId { get; set; }

    public CreateCheckListItemCommand(Guid checkListId, string description, DateTime? dueDate, int cardId)
    {
        CheckListId = checkListId;
        Description = description;
        DueDate = dueDate;
        CardId = cardId;
    }
}
