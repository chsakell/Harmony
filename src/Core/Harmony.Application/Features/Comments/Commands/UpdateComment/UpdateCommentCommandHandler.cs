using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Messaging;

namespace Harmony.Application.Features.Comments.Commands.UpdateComment
{
    public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, Result<bool>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ICardRepository _cardRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IUserService _userService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IStringLocalizer<UpdateCommentCommandHandler> _localizer;

        public UpdateCommentCommandHandler(ICurrentUserService currentUserService,
            ICardRepository cardRepository,
            ICommentRepository commentRepository,
            IUserService userService,
            INotificationsPublisher notificationsPublisher,
            IStringLocalizer<UpdateCommentCommandHandler> localizer)
        {
            _currentUserService = currentUserService;
            _cardRepository = cardRepository;
            _commentRepository = commentRepository;
            _userService = userService;
            _notificationsPublisher = notificationsPublisher;
            _localizer = localizer;
        }
        public async Task<Result<bool>> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var comment = await _commentRepository.GetComment(request.CommentId);

            if(comment == null)
            {
                return await Result<bool>.FailAsync(_localizer["Comment not found"]);
            }

            if(comment.UserId != userId)
            {
                return await Result<bool>.FailAsync(_localizer["You can only edit your own comments"]);
            }

            comment.Text = request.Text;

            var dbResult = await _commentRepository.Update(comment);

            if (dbResult > 0)
            {
                return await Result<bool>.SuccessAsync(true, _localizer["Comment updated"]);
            }

            return await Result<bool>.FailAsync(_localizer["Failed to update comment"]);
        }
    }
}
