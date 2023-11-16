using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.MoveCard;

public class MoveCardCommand : IRequest<Result<CardDto>>
{
    public Guid CardId { get; set; }
    public Guid ListId { get; set; }
    public byte Position { get; set; }

	public MoveCardCommand(Guid cardId, Guid listId, byte position)
	{
		CardId = cardId;
		ListId = listId;
		Position = position;
	}
}
