using Harmony.Application.DTO;
using Harmony.Domain.Entities;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Cards.Commands.CreateCheckListItem;

public class CreateCheckListItemCommand : IRequest<Result<CheckListItemDto>>
{
    public string Description { get; set; }
    public Guid CheckListId { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid CardId { get; set; }

    public CreateCheckListItemCommand(Guid checkListId, string description, DateTime? dueDate, Guid cardId)
    {
        CheckListId = checkListId;
        Description = description;
        DueDate = dueDate;
        CardId = cardId;
    }
}
