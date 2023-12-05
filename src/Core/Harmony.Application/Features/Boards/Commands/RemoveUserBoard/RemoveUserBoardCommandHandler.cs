using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications;
using Harmony.Shared.Utilities;

namespace Harmony.Application.Features.Boards.Commands.RemoveUserBoard
{
    /// <summary>
    /// Handler for removing a member for a board
    /// </summary>
    public class RemoveUserBoardCommandHandler : IRequestHandler<RemoveUserBoardCommand, Result<RemoveUserBoardResponse>>
    {
        private readonly IUserBoardRepository _userBoardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IBoardRepository _boardRepository;
        private readonly IStringLocalizer<RemoveUserBoardCommandHandler> _localizer;

        public RemoveUserBoardCommandHandler(IUserBoardRepository userBoardRepository,
            ICurrentUserService currentUserService,
            INotificationsPublisher notificationsPublisher,
            IBoardRepository boardRepository,
            IStringLocalizer<RemoveUserBoardCommandHandler> localizer)
        {
            _userBoardRepository = userBoardRepository;
            _currentUserService = currentUserService;
            _notificationsPublisher = notificationsPublisher;
            _boardRepository = boardRepository;
            _localizer = localizer;
        }
        public async Task<Result<RemoveUserBoardResponse>> Handle(RemoveUserBoardCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<RemoveUserBoardResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userBoard = await _userBoardRepository.GetUserBoard(request.BoardId, request.UserId);

            if(userBoard == null)
            {
                return await Result<RemoveUserBoardResponse>.FailAsync(_localizer["User doesn't belong to board"]);
            }

            var dbResult = await _userBoardRepository.Delete(userBoard);

            if (dbResult > 0)
            {
                var board = await _boardRepository.GetAsync(request.BoardId);

                var slug = StringUtilities.SlugifyString(board.Title.ToString());

                var boardUrl = $"{request.HostUrl}boards/{board.Id}/{slug}/";

                _notificationsPublisher
                    .Publish(new MemberRemovedFromBoardNotification(request.BoardId, request.UserId, boardUrl));

                var result = new RemoveUserBoardResponse(request.BoardId, userBoard.UserId);

                return await Result<RemoveUserBoardResponse>.SuccessAsync(result, _localizer["User removed from board"]);
            }

            return await Result<RemoveUserBoardResponse>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
