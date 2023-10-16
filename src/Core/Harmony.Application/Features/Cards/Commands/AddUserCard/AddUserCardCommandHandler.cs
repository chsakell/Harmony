using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;

namespace Harmony.Application.Features.Cards.Commands.AddUserCard
{
    public class AddUserCardCommandHandler : IRequestHandler<AddUserCardCommand, Result<AddUserCardResponse>>
    {
        private readonly IUserBoardRepository _userBoardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserCardRepository _userCardRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IUserService _userService;
        private readonly IStringLocalizer<AddUserCardCommandHandler> _localizer;

        public AddUserCardCommandHandler(IUserBoardRepository userBoardRepository,
            ICurrentUserService currentUserService,
            IUserCardRepository userCardRepository,
            ICardRepository cardRepository,
            IUserService userService,
            IStringLocalizer<AddUserCardCommandHandler> localizer)
        {
            _userBoardRepository = userBoardRepository;
            _currentUserService = currentUserService;
            _userCardRepository = userCardRepository;
            _cardRepository = cardRepository;
            _userService = userService;
            _localizer = localizer;
        }
        public async Task<Result<AddUserCardResponse>> Handle(AddUserCardCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<AddUserCardResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var boardId = await _cardRepository.GetBoardId(request.CardId);
            var userBoard = await _userBoardRepository.GetUserBoard(boardId, request.UserId);
            var user = (await _userService.GetAsync(request.UserId)).Data;

            var userAlreadyBelongsToBoard = userBoard != null;

            var userCard = await _userCardRepository.GetUserCard(request.CardId, request.UserId);

            if (userCard == null)
            {
                userCard = new UserCard()
                {
                    CardId = request.CardId,
                    UserId = request.UserId
                };

                if (!userAlreadyBelongsToBoard)
                {
                    userBoard = new UserBoard()
                    {
                        UserId = request.UserId,
                        BoardId = boardId,
                        Access = Domain.Enums.UserBoardAccess.Member
                    };

                    await _userBoardRepository.AddAsync(userBoard);
                }

                var dbResult = await _userCardRepository.CreateAsync(userCard);

                if (dbResult > 0)
                {
                    var result = new AddUserCardResponse(request.CardId, request.UserId)
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    };

                    return await Result<AddUserCardResponse>.SuccessAsync(result, _localizer["User added to card"]);
                }
            }

            return await Result<AddUserCardResponse>.FailAsync(_localizer["User is already a card's member"]);
        }
    }
}
