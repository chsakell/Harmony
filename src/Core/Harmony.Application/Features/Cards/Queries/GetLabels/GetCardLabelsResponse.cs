using Harmony.Application.DTO;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Cards.Queries.GetLabels
{
    public class GetCardLabelsResponse
    {
        public List<LabelDto> BoardLabels { get; set; } = new List<LabelDto>();
        public List<Guid> CardLabels { get; set; } = new List<Guid>();
    }
}
