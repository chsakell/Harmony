using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.RemoveUserCard
{
    public class RemoveUserCardCommand : IRequest<Result<RemoveUserCardResponse>>
    {
        public RemoveUserCardCommand(Guid cardId, string userId)
        {
            CardId = cardId;
            UserId = userId;
        }

        public Guid CardId { get; set; }
        public string UserId { get; set; }
        public string HostUrl { get; set; }
    }
}
