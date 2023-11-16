using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Queries.GetLabels
{
    /// <summary>
    /// Query for returning card labels
    /// </summary>
    public class GetCardLabelsQuery : IRequest<IResult<List<LabelDto>>>
    {
        public Guid CardId { get; set; }

        public GetCardLabelsQuery(Guid cardId)
        {
            CardId = cardId;
        }
    }
}