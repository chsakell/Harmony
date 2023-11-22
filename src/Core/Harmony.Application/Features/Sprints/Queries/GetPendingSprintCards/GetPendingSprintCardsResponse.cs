using Harmony.Application.DTO;
using Harmony.Application.Responses;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Boards.Queries.GetSprints
{
    public class GetPendingSprintCardsResponse
    {
        public List<CardDto> PendingCards { get; set; }
        public List<SprintDto> AvailableSprints { get; set; }
    }
}
