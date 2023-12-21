using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Application.DTO;
using AutoMapper;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications;
using Harmony.Shared.Utilities;
using Harmony.Application.Notifications.Email;

namespace Harmony.Application.Features.Cards.Commands.AddUserCard
{
    public class AddUserCardCommandHandler : IRequestHandler<AddUserCardCommand, Result<AddUserCardResponse>>
    {
        private readonly IUserBoardRepository _userBoardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserCardRepository _userCardRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IHubClientNotifierService _hubClientNotifierService;
        private readonly IStringLocalizer<AddUserCardCommandHandler> _localizer;

        public AddUserCardCommandHandler(IUserBoardRepository userBoardRepository,
            ICurrentUserService currentUserService,
            IUserCardRepository userCardRepository,
            ICardRepository cardRepository,
            IUserService userService, IMapper mapper,
            INotificationsPublisher notificationsPublisher,
            IHubClientNotifierService hubClientNotifierService,
            IStringLocalizer<AddUserCardCommandHandler> localizer)
        {
            _userBoardRepository = userBoardRepository;
            _currentUserService = currentUserService;
            _userCardRepository = userCardRepository;
            _cardRepository = cardRepository;
            _userService = userService;
            _mapper = mapper;
            _notificationsPublisher = notificationsPublisher;
            _hubClientNotifierService = hubClientNotifierService;
            _localizer = localizer;
        }
        public async Task<Result<AddUserCardResponse>> Handle(AddUserCardCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<AddUserCardResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userBoard = await _userBoardRepository.GetUserBoard(request.BoardId, request.UserId);
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
                        BoardId = request.BoardId,
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

                    var member = _mapper.Map<CardMemberDto>(user);

                    await _userBoardRepository.LoadBoardEntryAsync(userBoard);

                    var slug = StringUtilities.SlugifyString(userBoard.Board.Title.ToString());

                    var cardUrl = $"{request.HostUrl}boards/{userBoard.Board.Id}/{slug}/?cardId={request.CardId}";
                    
                    _notificationsPublisher.Publish(new MemberAddedToCardNotification(request.BoardId, request.CardId, request.UserId , cardUrl));

                    await _hubClientNotifierService.AddCardMember(request.BoardId, request.CardId, member);

                    return await Result<AddUserCardResponse>.SuccessAsync(result, _localizer["User added to card"]);
                }
            }

            return await Result<AddUserCardResponse>.FailAsync(_localizer["User is already a card's member"]);
        }
    }
}
