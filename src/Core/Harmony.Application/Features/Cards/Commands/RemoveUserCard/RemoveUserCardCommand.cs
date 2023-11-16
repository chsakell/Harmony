using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.RemoveUserCard
{
    public class RemoveUserCardCommand : IRequest<Result<RemoveUserCardResponse>>
    {
        public RemoveUserCardCommand(int cardId, string userId)
        {
            CardId = cardId;
            UserId = userId;
        }

        public int CardId { get; set; }
        public string UserId { get; set; }
    }
}
