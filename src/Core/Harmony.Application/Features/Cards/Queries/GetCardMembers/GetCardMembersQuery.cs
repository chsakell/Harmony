using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Queries.GetCardMembers
{
    public class GetCardMembersQuery : IRequest<Result<List<CardMemberResponse>>>
    {
        public Guid CardId { get; set; }

        public GetCardMembersQuery(Guid cardId)
        {
            CardId = cardId;
        }
    }
}