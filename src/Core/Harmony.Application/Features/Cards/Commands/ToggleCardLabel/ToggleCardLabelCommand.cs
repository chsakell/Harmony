using Harmony.Application.DTO;
using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.ToggleCardLabel;

public class ToggleCardLabelCommand : BaseBoardCommand, IRequest<Result<LabelDto>>
{
    public Guid CardId { get; set; }
    public Guid LabelId { get; set; }

	public ToggleCardLabelCommand(Guid cardId, Guid labelId)
	{
		CardId = cardId;
        LabelId = labelId;
	}
}
