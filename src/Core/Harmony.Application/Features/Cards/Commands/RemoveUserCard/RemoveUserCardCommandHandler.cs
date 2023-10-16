using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;

namespace Harmony.Application.Features.Cards.Commands.RemoveUserCard
{
    public class RemoveUserCardCommandHandler : IRequestHandler<RemoveUserCardCommand, Result<RemoveUserCardResponse>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserCardRepository _userCardRepository;
        private readonly IUserService _userService;
        private readonly IStringLocalizer<RemoveUserCardCommandHandler> _localizer;

        public RemoveUserCardCommandHandler(ICurrentUserService currentUserService,
            IUserCardRepository userCardRepository,
            IUserService userService,
            IStringLocalizer<RemoveUserCardCommandHandler> localizer)
        {
            _currentUserService = currentUserService;
            _userCardRepository = userCardRepository;
            _userService = userService;
            _localizer = localizer;
        }
        public async Task<Result<RemoveUserCardResponse>> Handle(RemoveUserCardCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<RemoveUserCardResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var user = (await _userService.GetAsync(request.UserId)).Data;

            var userCard = await _userCardRepository.GetUserCard(request.CardId, request.UserId);

            if (userCard != null)
            {
                var dbResult = await _userCardRepository.Delete(userCard);

                if (dbResult > 0)
                {
                    var result = new RemoveUserCardResponse(request.CardId, request.UserId)
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    };

                    return await Result<RemoveUserCardResponse>.SuccessAsync(result, _localizer["User removed from card"]);
                }
            }

            return await Result<RemoveUserCardResponse>.FailAsync(_localizer["User doesn't belong to card"]);
        }
    }
}
