using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Application.DTO;
using AutoMapper;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications;
using Harmony.Domain.Entities;
using Harmony.Shared.Utilities;
using Harmony.Application.Notifications.Email;

namespace Harmony.Application.Features.Cards.Commands.RemoveUserCard
{
    public class RemoveUserCardCommandHandler : IRequestHandler<RemoveUserCardCommand, Result<RemoveUserCardResponse>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IHubClientNotifierService _hubClientNotifierService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IUserCardRepository _userCardRepository;
        private readonly IBoardRepository _boardRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<RemoveUserCardCommandHandler> _localizer;

        public RemoveUserCardCommandHandler(ICurrentUserService currentUserService,
            IHubClientNotifierService hubClientNotifierService,
            INotificationsPublisher notificationsPublisher,
            IUserCardRepository userCardRepository,
            IBoardRepository boardRepository,
            ICardRepository cardRepository,
            IUserService userService,
            IMapper mapper,
            IStringLocalizer<RemoveUserCardCommandHandler> localizer)
        {
            _currentUserService = currentUserService;
            _hubClientNotifierService = hubClientNotifierService;
            _notificationsPublisher = notificationsPublisher;
            _userCardRepository = userCardRepository;
            _boardRepository = boardRepository;
            _cardRepository = cardRepository;
            _userService = userService;
            _mapper = mapper;
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

                    await _hubClientNotifierService.RemoveCardMember(request.BoardId, request.CardId, member);

                    var board = await _boardRepository.GetAsync(request.BoardId);

                    var slug = StringUtilities.SlugifyString(board.Title.ToString());

                    var cardUrl = $"{request.HostUrl}boards/{board.Id}/{slug}/?cardId={request.CardId}";

                    _notificationsPublisher.Publish(new MemberRemovedFromCardNotification(request.BoardId, request.CardId, request.UserId, cardUrl));

                    return await Result<RemoveUserCardResponse>.SuccessAsync(result, _localizer["User removed from card"]);
                }
            }

            return await Result<RemoveUserCardResponse>.FailAsync(_localizer["User doesn't belong to card"]);
        }
    }
}
