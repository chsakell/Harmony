using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Queries.LoadCard
{
    /// <summary>
    /// Query for loading card
    /// </summary>
    public class LoadCardQuery : IRequest<IResult<LoadCardResponse>>
    {
        public int CardId { get; set; }

        public LoadCardQuery(int cardId)
        {
            CardId = cardId;
        }
    }
}