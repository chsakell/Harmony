using Harmony.Application.Models;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardDates;

public class UpdateCardDatesCommand : BaseBoardCommand, IRequest<Result<bool>>
{
    public UpdateCardDatesCommand(Guid cardId, DateTime? startDate, DateTime? dueDate, DueDateReminderType? dueDateReminderType)
    {
        CardId = cardId;
        StartDate = startDate;
        DueDate = dueDate;
        DueDateReminderType = dueDateReminderType;
    }

    public Guid CardId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DueDateReminderType? DueDateReminderType { get; set; }
}
