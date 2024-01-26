using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Messaging;

namespace Harmony.Application.Features.Comments.Commands.DeleteComment
{
    public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Result<bool>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ICommentRepository _commentRepository;
        private readonly IUserService _userService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IStringLocalizer<DeleteCommentCommandHandler> _localizer;

        public DeleteCommentCommandHandler(ICurrentUserService currentUserService,
            ICardRepository cardRepository,
            ICommentRepository commentRepository,
            IUserService userService,
            INotificationsPublisher notificationsPublisher,
            IStringLocalizer<DeleteCommentCommandHandler> localizer)
        {
            _currentUserService = currentUserService;
            _commentRepository = commentRepository;
            _userService = userService;
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
                return await Result<bool>.SuccessAsync(true, _localizer["Comment removed"]);
            }

            return await Result<bool>.FailAsync(_localizer["Failed to remove comment"]);
        }
    }
}
