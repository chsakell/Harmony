using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Application.Features.Lists.Queries.LoadBoardList;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Comments.Queries.GetCardComments
{
    /// <summary>
    /// Handler for returning board lists
    /// </summary>
    public class GetCardCommentsCommandHandler : IRequestHandler<GetCardCommentsCommandQuery, IResult<List<CommentDto>>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ICommentService _commentService;
        private readonly IStringLocalizer<GetBoardListHandler> _localizer;

        public GetCardCommentsCommandHandler(ICurrentUserService currentUserService,
            ICommentService commentService,
            IStringLocalizer<GetBoardListHandler> localizer)
        {
            _currentUserService = currentUserService;
            _commentService = commentService;
            _localizer = localizer;
        }

        public async Task<IResult<List<CommentDto>>> Handle(GetCardCommentsCommandQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {

                return await Result<List<CommentDto>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var comments = await _commentService.GetCommentsForCard(request.CardId, userId);

            return await Result<List<CommentDto>>.SuccessAsync(comments);
        }
    }
}
