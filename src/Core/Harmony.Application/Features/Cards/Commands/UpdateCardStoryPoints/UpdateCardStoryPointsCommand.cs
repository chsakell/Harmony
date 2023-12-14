using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardStoryPoints;

public class UpdateCardStoryPointsCommand : IRequest<Result<bool>>
{
    public UpdateCardStoryPointsCommand(Guid boardId, Guid cardId, short? storyPoints)
    {
        BoardId = boardId;
        CardId = cardId;
        StoryPoints = storyPoints;
    }

    public Guid BoardId { get; set; }
    public Guid CardId { get; set; }
    public short? StoryPoints { get; set; }
}
