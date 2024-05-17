using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Harmony.Application.Contracts.Services.Caching;

namespace Harmony.Application.Features.Comments.Commands.DeleteComment
{
    public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Result<bool>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ICardSummaryService _cardSummaryService;
        private readonly ICommentRepository _commentRepository;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IStringLocalizer<DeleteCommentCommandHandler> _localizer;

        public DeleteCommentCommandHandler(ICurrentUserService currentUserService,
            ICardSummaryService cardSummaryService,
            ICommentRepository commentRepository,
            INotificationsPublisher notificationsPublisher,
            IStringLocalizer<DeleteCommentCommandHandler> localizer)
        {
            _currentUserService = currentUserService;
            _cardSummaryService = cardSummaryService;
            _commentRepository = commentRepository;
            _notificationsPublisher = notificationsPublisher;
            _localizer = localizer;
        }
        public async Task<Result<bool>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var comment = await _commentRepository.GetComment(request.CommentId);

            if (comment == null)
            {
                return await Result<bool>.FailAsync(_localizer["Comment not found"]);
            }

            if (comment.UserId != userId)
            {
                return await Result<bool>.FailAsync(_localizer["You can only remove your own comments"]);
            }

            var dbResult = await _commentRepository.Delete(comment);
            if (dbResult > 0)
            {
                await _cardSummaryService.UpdateCardSummary(request.BoardId, request.CardId,
                    (summary) =>
                    {
                        summary.TotalComments -= 1;
                    });

                var cardCommentDeletedMessage = new CardCommentDeletedMessage()
                {
                    BoardId = request.BoardId,
                    CardId = request.CardId,
                };

                _notificationsPublisher.PublishMessage(cardCommentDeletedMessage,
                    NotificationType.CardCommentDeleted,
                    routingKey: BrokerConstants.RoutingKeys.SignalR);

                return await Result<bool>.SuccessAsync(true, _localizer["Comment removed"]);
            }

            return await Result<bool>.FailAsync(_localizer["Failed to remove comment"]);
        }
    }
}
