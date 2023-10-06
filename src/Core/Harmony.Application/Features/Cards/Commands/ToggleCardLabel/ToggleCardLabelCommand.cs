using Harmony.Application.DTO;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.ToggleCardLabel;

public class ToggleCardLabelCommand : IRequest<Result<LabelDto>>
{
    public Guid CardId { get; set; }
    public Guid LabelId { get; set; }

	public ToggleCardLabelCommand(Guid cardId, Guid labelId)
	{
		CardId = cardId;
        LabelId = labelId;
	}
}
