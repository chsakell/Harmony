using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Shared.Constants.Application;
using Harmony.Domain.Enums;
using Harmony.Application.Features.Boards.Queries.SearchBoardUsers;
using Harmony.Application.Responses;
using static Harmony.Shared.Constants.Permission.Permissions;
using Harmony.Application.Features.Boards.Queries.GetBoardUsers;

namespace Harmony.Application.Features.Boards.Commands.AddUserBoard
{
    public class AddUserBoardCommandHandler : IRequestHandler<AddUserBoardCommand, Result<UserBoardResponse>>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IUserBoardRepository _userBoardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBoardLabelRepository _boardLabelRepository;
        private readonly IStringLocalizer<AddUserBoardCommandHandler> _localizer;

        public AddUserBoardCommandHandler(IBoardRepository boardRepository,
            IUserBoardRepository userBoardRepository,
            ICurrentUserService currentUserService,
            IBoardLabelRepository boardLabelRepository,
            IStringLocalizer<AddUserBoardCommandHandler> localizer)
        {
            _boardRepository = boardRepository;
            _userBoardRepository = userBoardRepository;
            _currentUserService = currentUserService;
            _boardLabelRepository = boardLabelRepository;
            _localizer = localizer;
        }
        public async Task<Result<UserBoardResponse>> Handle(AddUserBoardCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<UserBoardResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userBoard = new UserBoard()
            {
                UserId = request.UserId,
                BoardId = request.BoardId,
                Access = request.Access
            };

            var dbResult = await _userBoardRepository.CreateAsync(userBoard);

            if (dbResult > 0)
            {
                var user = await _userBoardRepository.GetBoardAccessMember(request.BoardId, request.UserId);

                return await Result<UserBoardResponse>.SuccessAsync(user, _localizer["User added to board"]);
            }

            return await Result<UserBoardResponse>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
