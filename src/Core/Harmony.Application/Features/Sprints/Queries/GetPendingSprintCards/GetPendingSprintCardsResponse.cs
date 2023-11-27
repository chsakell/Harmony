using Harmony.Application.DTO;

namespace Harmony.Application.Features.Boards.Queries.GetSprints
{
    public class GetPendingSprintCardsResponse
    {
        public List<CardDto> PendingCards { get; set; }
        public List<SprintDto> AvailableSprints { get; set; }
    }
}
