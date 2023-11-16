using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Queries.GetCardMembers
{
    public class GetCardMembersQuery : IRequest<Result<List<CardMemberResponse>>>
    {
        public int CardId { get; set; }

        public GetCardMembersQuery(int cardId)
        {
            CardId = cardId;
        }
    }
}