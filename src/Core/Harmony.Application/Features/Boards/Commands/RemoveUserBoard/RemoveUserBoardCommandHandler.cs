using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;

namespace Harmony.Application.Features.Boards.Commands.RemoveUserBoard
{
    /// <summary>
    /// Handler for removing a member for a board
    /// </summary>
    public class RemoveUserBoardCommandHandler : IRequestHandler<RemoveUserBoardCommand, Result<RemoveUserBoardResponse>>
    {
        private readonly IUserBoardRepository _userBoardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<RemoveUserBoardCommandHandler> _localizer;

        public RemoveUserBoardCommandHandler(IUserBoardRepository userBoardRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<RemoveUserBoardCommandHandler> localizer)
        {
            _userBoardRepository = userBoardRepository;
            _currentUserService = currentUserService;
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
                var result = new RemoveUserBoardResponse(request.BoardId, userBoard.UserId);

                return await Result<RemoveUserBoardResponse>.SuccessAsync(result, _localizer["User removed from board"]);
            }

            return await Result<RemoveUserBoardResponse>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
