using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
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

namespace Harmony.Application.Features.Cards.Commands.RemoveUserCard
{
    public class RemoveUserCardCommandHandler : IRequestHandler<RemoveUserCardCommand, Result<RemoveUserCardResponse>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IUserCardRepository _userCardRepository;
        private readonly IUserService _userService;
        private readonly IBoardService _boardService;
        private readonly IMapper _mapper;
        private readonly IOptions<AppEndpointConfiguration> _endpointsConfiguration;
        private readonly IStringLocalizer<RemoveUserCardCommandHandler> _localizer;

        public RemoveUserCardCommandHandler(ICurrentUserService currentUserService,
            INotificationsPublisher notificationsPublisher,
            IUserCardRepository userCardRepository,
            IUserService userService,
            IBoardService boardService,
            IMapper mapper, IOptions<AppEndpointConfiguration> endpointsConfiguration,
            IStringLocalizer<RemoveUserCardCommandHandler> localizer)
        {
            _currentUserService = currentUserService;
            _notificationsPublisher = notificationsPublisher;
            _userCardRepository = userCardRepository;
            _userService = userService;
            _boardService = boardService;
            _mapper = mapper;
            _endpointsConfiguration = endpointsConfiguration;
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
                    var member = _mapper.Map<CardMemberDto>(user);

                    var message = new CardMemberRemovedMessage(request.BoardId, request.CardId, member);

                    _notificationsPublisher.PublishMessage(message,
                        NotificationType.CardMemberRemoved, routingKey: BrokerConstants.RoutingKeys.SignalR);

                    var board = await _boardService.GetBoardInfo(request.BoardId);

                    var slug = StringUtilities.SlugifyString(board.Title.ToString());

                    var cardUrl = $"{_endpointsConfiguration.Value.FrontendUrl}/boards/{board.Id}/{slug}/{request.CardId}";

                    _notificationsPublisher.PublishEmailNotification(new MemberRemovedFromCardNotification(request.BoardId, request.CardId, request.UserId, cardUrl));
                    
                    var members = await _userCardRepository.GetCardMembers(request.CardId);

                    _notificationsPublisher
                            .PublishSearchIndexNotification(new CardMembersUpdatedIndexNotification()
                            {
                                ObjectID = request.CardId.ToString(),
                                Members = members
                            }, board.IndexName);

                    return await Result<RemoveUserCardResponse>.SuccessAsync(result, _localizer["User removed from card"]);
                }
            }

            return await Result<RemoveUserCardResponse>.FailAsync(_localizer["User doesn't belong to card"]);
        }
    }
}
