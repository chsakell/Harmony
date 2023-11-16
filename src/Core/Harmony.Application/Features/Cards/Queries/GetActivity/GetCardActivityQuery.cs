using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Queries.GetActivity
{
    public class GetCardActivityQuery : IRequest<IResult<List<CardActivityDto>>>
    {
        public int CardId { get; set; }

        public GetCardActivityQuery(int cardId)
        {
            CardId = cardId;
        }
    }
}