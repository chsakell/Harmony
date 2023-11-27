using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Features.Boards.Commands.UpdateUserBoardAccess;

namespace Harmony.Application.Features.Boards.Commands.SetUserBoardAccess
{
    /// <summary>
    /// Handler to assign access level to a board member
    /// </summary>
    public class UpdateUserBoardAccessCommandHandler : IRequestHandler<UpdateUserBoardAccessCommand, Result<UpdateUserBoardAccessResponse>>
    {
        private readonly IUserBoardRepository _userBoardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<UpdateUserBoardAccessCommandHandler> _localizer;

        public UpdateUserBoardAccessCommandHandler(
            IUserBoardRepository userBoardRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<UpdateUserBoardAccessCommandHandler> localizer)
        {
            _userBoardRepository = userBoardRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<UpdateUserBoardAccessResponse>> Handle(UpdateUserBoardAccessCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<UpdateUserBoardAccessResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userBoard = await _userBoardRepository.GetUserBoard(request.BoardId, request.UserId);

            if(userBoard == null)
            {
                return await Result<UpdateUserBoardAccessResponse>.FailAsync(_localizer["User isn't member of the board"]);
            }

            userBoard.Access = request.Access;

            var dbResult = await _userBoardRepository.Update(userBoard);

            if (dbResult > 0)
            {
                var user = await _userBoardRepository.GetBoardAccessMember(request.BoardId, request.UserId);

                var result = new UpdateUserBoardAccessResponse()
                {
                    UserId = request.UserId,
                    BoardId = request.BoardId,
                    Access = request.Access,
                    FullName = $"{user.FullName}"
                };

                return await Result<UpdateUserBoardAccessResponse>
                    .SuccessAsync(result, _localizer[$"{user.UserName} access level changed to {request.Access}"]);
            }

            return await Result<UpdateUserBoardAccessResponse>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
