using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardTitle;

public class UpdateCardTitleCommand : BaseBoardCommand, IRequest<Result<bool>>
{
    public Guid CardId { get; set; }
    public string Title { get; set; }

	public UpdateCardTitleCommand(Guid cardId, string title)
	{
		CardId = cardId;
        Title = title;
	}
}
