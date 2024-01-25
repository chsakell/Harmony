using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.AddUserCard
{
    public class AddUserCardCommand : BaseBoardCommand, IRequest<Result<AddUserCardResponse>>
    {
        public AddUserCardCommand(Guid cardId, string userId)
        {
            CardId = cardId;
            UserId = userId;
        }

        public Guid CardId { get; set; }
        public string UserId { get; set; }
    }
}
