using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Boards.Commands.CreateCard;
using Harmony.Application.Features.Boards.Commands.CreateList;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetAllForUser;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface ICardManager : IManager
    {
        Task<IResult<CardDto>> CreateCardAsync(CreateCardCommand request);
	}
}
