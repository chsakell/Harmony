using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Application.Features.Workspaces.Commands.AddMember;

namespace Harmony.Application.Features.Boards.Commands.AddUserBoard
{
    public class AddUserBoardCommandHandler : IRequestHandler<AddUserBoardCommand, Result<UserBoardResponse>>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IUserBoardRepository _userBoardRepository;
        private readonly IUserWorkspaceRepository _userWorkspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBoardLabelRepository _boardLabelRepository;
        private readonly ISender _sender;
        private readonly IStringLocalizer<AddUserBoardCommandHandler> _localizer;

        public AddUserBoardCommandHandler(IBoardRepository boardRepository,
            IUserBoardRepository userBoardRepository,
            IUserWorkspaceRepository userWorkspaceRepository,
            ICurrentUserService currentUserService,
            IBoardLabelRepository boardLabelRepository,
            ISender sender,
            IStringLocalizer<AddUserBoardCommandHandler> localizer)
        {
            _boardRepository = boardRepository;
            _userBoardRepository = userBoardRepository;
            _userWorkspaceRepository = userWorkspaceRepository;
            _currentUserService = currentUserService;
            _boardLabelRepository = boardLabelRepository;
            _sender = sender;
            _localizer = localizer;
        }
        public async Task<Result<UserBoardResponse>> Handle(AddUserBoardCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<UserBoardResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userBoard = await _userBoardRepository.GetUserBoard(request.BoardId, request.UserId);

            if(userBoard != null)
            {
                return await Result<UserBoardResponse>.FailAsync(_localizer["User has already access to the board"]);
            }

            var boardWorkSpace = await _userBoardRepository.GetWorkspace(request.BoardId);

            var userWorkspace = await _userWorkspaceRepository.GetUserWorkspace(boardWorkSpace.Id, request.UserId);

            if (userWorkspace == null)
            {
                var addWorkspaceMemberResult = await _sender.Send(new AddWorkspaceMemberCommand(request.UserId, boardWorkSpace.Id));

                if (!addWorkspaceMemberResult.Succeeded)
                {
                    return await Result<UserBoardResponse>.FailAsync(_localizer["Operation failed during adding user to workspace"]);
                }
            }

            userBoard = new UserBoard()
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
