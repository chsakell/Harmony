using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.DTO;
using AutoMapper;
using Harmony.Application.Contracts.Messaging;
using Harmony.Shared.Utilities;
using Harmony.Application.Notifications.Email;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Application.Constants;
using Harmony.Application.Notifications;
using Harmony.Domain.Enums;
using Harmony.Application.Configurations;
using Microsoft.Extensions.Options;

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
        private readonly IBoardService _boardService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IOptions<AppEndpointConfiguration> _endpointsConfiguration;
        private readonly IStringLocalizer<AddUserCardCommandHandler> _localizer;

        public AddUserCardCommandHandler(IUserBoardRepository userBoardRepository,
            ICurrentUserService currentUserService,
            IUserCardRepository userCardRepository,
            ICardRepository cardRepository,
            IUserService userService, IMapper mapper,
            IBoardService boardService,
            INotificationsPublisher notificationsPublisher,
            IOptions<AppEndpointConfiguration> endpointsConfiguration,
            IStringLocalizer<AddUserCardCommandHandler> localizer)
        {
            _userBoardRepository = userBoardRepository;
            _currentUserService = currentUserService;
            _userCardRepository = userCardRepository;
            _cardRepository = cardRepository;
            _userService = userService;
            _mapper = mapper;
            _boardService = boardService;
            _notificationsPublisher = notificationsPublisher;
            _endpointsConfiguration = endpointsConfiguration;
            _localizer = localizer;
        }
        public async Task<Result<AddUserCardResponse>> Handle(AddUserCardCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId) && !_currentUserService.IsTrustedClientRequest)
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
                        Access = UserBoardAccess.Member
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

                    var cardUrl = $"{_endpointsConfiguration.Value.FrontendUrl}/boards/{userBoard.Board.Id}/{slug}/{request.CardId}";
                    
                    _notificationsPublisher.PublishEmailNotification(new MemberAddedToCardNotification(request.BoardId, request.CardId, request.UserId , cardUrl));

                    var message = new CardMemberAddedMessage(request.BoardId, request.CardId, member);

                    _notificationsPublisher.PublishMessage(message,
                        NotificationType.CardMemberAdded, routingKey: BrokerConstants.RoutingKeys.SignalR);

                    var board = await _boardService.GetBoardInfo(request.BoardId);
                    var members = await _userCardRepository.GetCardMembers(request.CardId);

                    _notificationsPublisher
                            .PublishSearchIndexNotification(new CardMembersUpdatedIndexNotification()
                            {
                                ObjectID = request.CardId.ToString(),
                                Members = members
                            }, board.IndexName);

                    return await Result<AddUserCardResponse>.SuccessAsync(result, _localizer["User added to card"]);
                }
            }

            return await Result<AddUserCardResponse>.FailAsync(_localizer["User is already a card's member"]);
        }
    }
}
