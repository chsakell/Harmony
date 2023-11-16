using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.AddUserCard
{
    public class AddUserCardCommand : IRequest<Result<AddUserCardResponse>>
    {
        public AddUserCardCommand(int cardId, string userId)
        {
            CardId = cardId;
            UserId = userId;
        }

        public int CardId { get; set; }
        public string UserId { get; set; }
    }
}
