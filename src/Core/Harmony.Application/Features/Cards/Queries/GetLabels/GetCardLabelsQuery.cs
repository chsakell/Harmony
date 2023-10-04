using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Queries.GetLabels
{
    public class GetCardLabelsQuery : IRequest<IResult<GetCardLabelsResponse>>
    {
        public Guid CardId { get; set; }

        public GetCardLabelsQuery(Guid cardId)
        {
            CardId = cardId;
        }
    }
}