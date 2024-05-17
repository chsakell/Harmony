using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Constants;
using Harmony.Application.Notifications;
using Harmony.Domain.Enums;
using Harmony.Application.DTO.Summaries;
using Harmony.Domain.Extensions;
using System.Text.Json;

namespace Harmony.Application.Features.Comments.Commands.CreateComment
{
    public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Result<CreateCommentResponse>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ICommentRepository _commentRepository;
        private readonly IUserService _userService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly ICacheService _cacheService;
        private readonly IStringLocalizer<CreateCommentCommandHandler> _localizer;

        public CreateCommentCommandHandler(ICurrentUserService currentUserService,
            ICommentRepository commentRepository,
            IUserService userService,
            INotificationsPublisher notificationsPublisher,
            ICacheService cacheService,
            IStringLocalizer<CreateCommentCommandHandler> localizer)
        {
            _currentUserService = currentUserService;
            _commentRepository = commentRepository;
            _userService = userService;
            _notificationsPublisher = notificationsPublisher;
            _cacheService = cacheService;
            _localizer = localizer;
        }
        public async Task<Result<CreateCommentResponse>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<CreateCommentResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var comment = new Comment()
            {
                CardId = request.CardId,
                Text = request.Text,
                UserId = userId,
            };

            var dbResult = await _commentRepository.CreateAsync(comment);

            if (dbResult > 0)
            {
                var cardSummary = await _cacheService.HashGetAsync<CardSummary>(
                        CacheKeys.ActiveCardSummaries(request.BoardId),
                        request.CardId.ToString());

                if (cardSummary != null)
                {
                    cardSummary.TotalComments += 1;

                    await _cacheService.HashHSetAsync(CacheKeys.ActiveCardSummaries(request.BoardId),
                    request.CardId.ToString(), JsonSerializer.Serialize(cardSummary, CacheDomainExtensions._jsonSerializerOptions));
                }

                var user = (await _userService.GetPublicInfoAsync(userId)).Data;

                var result = new CreateCommentResponse()
                {
                    Id = comment.Id,
                    User = user,
                    DateCreated = comment.DateCreated
                };

                var cardCommentCreatedMessage = new CardCommentCreatedMessage()
                {
                    BoardId = request.BoardId,
                    CardId = request.CardId,
                };

                _notificationsPublisher.PublishMessage(cardCommentCreatedMessage,
                    NotificationType.CardCommentCreated, 
                    routingKey: BrokerConstants.RoutingKeys.SignalR);

                return await Result<CreateCommentResponse>.SuccessAsync(result, _localizer["Comment added to card"]);
            }

            return await Result<CreateCommentResponse>.FailAsync(_localizer["Failed to add comment"]);
        }
    }
}
