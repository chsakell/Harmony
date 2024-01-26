using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.DTO;
using Harmony.Application.Contracts.Messaging;

namespace Harmony.Application.Features.Comments.Commands.CreateComment
{
    public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Result<CreateCommentResponse>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ICardRepository _cardRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IUserService _userService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IStringLocalizer<CreateCommentCommandHandler> _localizer;

        public CreateCommentCommandHandler(ICurrentUserService currentUserService,
            ICardRepository cardRepository,
            ICommentRepository commentRepository,
            IUserService userService,
            INotificationsPublisher notificationsPublisher,
            IStringLocalizer<CreateCommentCommandHandler> localizer)
        {
            _currentUserService = currentUserService;
            _cardRepository = cardRepository;
            _commentRepository = commentRepository;
            _userService = userService;
            _notificationsPublisher = notificationsPublisher;
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
                var user = (await _userService.GetPublicInfoAsync(userId)).Data;

                var result2 = new CommentDto()
                {
                    Id = comment.Id,
                    Text = comment.Text,
                    User = user,
                    DateCreated = comment.DateCreated
                };

                var result = new CreateCommentResponse()
                {
                    Id = comment.Id,
                    User = user,
                    DateCreated = comment.DateCreated
                };

                return await Result<CreateCommentResponse>.SuccessAsync(result, _localizer["Comment added to card"]);
            }

            return await Result<CreateCommentResponse>.FailAsync(_localizer["Failed to add comment"]);
        }
    }
}
