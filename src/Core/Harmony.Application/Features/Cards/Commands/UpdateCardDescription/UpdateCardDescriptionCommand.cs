using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardDescription;

public class UpdateCardDescriptionCommand : BaseBoardCommand, IRequest<Result<bool>>
{
    public Guid CardId { get; set; }
    public string Description { get; set; }

	public UpdateCardDescriptionCommand(Guid cardId, string description)
	{
		CardId = cardId;
        Description = description;
	}
}
