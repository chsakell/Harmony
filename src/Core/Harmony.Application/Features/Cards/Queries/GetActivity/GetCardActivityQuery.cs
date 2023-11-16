using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Queries.GetActivity
{
    public class GetCardActivityQuery : IRequest<IResult<List<CardActivityDto>>>
    {
        public Guid CardId { get; set; }

        public GetCardActivityQuery(Guid cardId)
        {
            CardId = cardId;
        }
    }
}