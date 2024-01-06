using Harmony.Application.DTO;
using Harmony.Application.Models;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.MoveCard;

public class MoveCardCommand : BaseBoardCommand, IRequest<Result<CardDto>>
{
    public Guid CardId { get; set; }
    public Guid? ListId { get; set; }
    public short? Position { get; set; }
	public CardStatus Status { get; set; }
    public Guid UpdateId { get; set; }

    public MoveCardCommand(Guid cardId, Guid? listId, short? position, CardStatus status, Guid updateId)
    {
        CardId = cardId;
        ListId = listId;
        Position = position;
        Status = status;
        UpdateId = updateId;
    }
}
