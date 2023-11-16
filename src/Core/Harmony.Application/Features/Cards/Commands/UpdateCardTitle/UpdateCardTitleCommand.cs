using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardTitle;

public class UpdateCardTitleCommand : IRequest<Result<bool>>
{
    public int CardId { get; set; }
    public string Title { get; set; }

	public UpdateCardTitleCommand(int cardId, string title)
	{
		CardId = cardId;
        Title = title;
	}
}
