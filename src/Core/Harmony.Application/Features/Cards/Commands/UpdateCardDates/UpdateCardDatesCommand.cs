using Harmony.Application.DTO;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardDates;

public class UpdateCardDatesCommand : IRequest<Result<bool>>
{
    public UpdateCardDatesCommand(Guid cardId, DateTime? startDate, DateTime? dueDate)
    {
        CardId = cardId;
        StartDate = startDate;
        DueDate = dueDate;
    }

    public Guid CardId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
}
