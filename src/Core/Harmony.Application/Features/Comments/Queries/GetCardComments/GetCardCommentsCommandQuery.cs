using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Comments.Queries.GetCardComments
{
    /// <summary>
    /// Query for returning card's comments
    /// </summary>
    public class GetCardCommentsCommandQuery : IRequest<IResult<List<CommentDto>>>
    {
        public Guid CardId { get; set; }

        public GetCardCommentsCommandQuery(Guid cardId)
        {
            CardId = cardId;
        }
    }
}