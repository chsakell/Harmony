using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Commands.CreateCard;
using Harmony.Application.Features.Cards.Commands.MoveCard;
using Harmony.Application.Features.Cards.Commands.UpdateCardDescription;
using Harmony.Application.Features.Cards.Queries.LoadCard;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface ICardManager : IManager
    {
        Task<IResult<LoadCardResponse>> LoadCardAsync(LoadCardQuery request);
        Task<IResult<CardDto>> CreateCardAsync(CreateCardCommand request);
        Task<IResult<CardDto>> MoveCardAsync(MoveCardCommand request);
        Task<IResult<bool>> UpdateDescriptionAsync(UpdateCardDescriptionCommand request);
    }
}
