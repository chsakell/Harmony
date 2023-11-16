using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.ToggleCardLabel;

public class ToggleCardLabelCommand : IRequest<Result<LabelDto>>
{
    public int CardId { get; set; }
    public Guid LabelId { get; set; }

	public ToggleCardLabelCommand(int cardId, Guid labelId)
	{
		CardId = cardId;
        LabelId = labelId;
	}
}
