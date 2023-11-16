using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Shared.Constants.Application;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Boards.Commands.Create
{
    /// <summary>
    /// Handler for creating boards
    /// </summary>
    public class CreateBoardCommandHandler : IRequestHandler<CreateBoardCommand, Result<Guid>>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IUserBoardRepository _userBoardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBoardLabelRepository _boardLabelRepository;
        private readonly IStringLocalizer<CreateBoardCommandHandler> _localizer;

        public CreateBoardCommandHandler(IBoardRepository boardRepository,
            IUserBoardRepository userBoardRepository,
            ICurrentUserService currentUserService,
            IBoardLabelRepository boardLabelRepository,
            IStringLocalizer<CreateBoardCommandHandler> localizer)
        {
            _boardRepository = boardRepository;
            _userBoardRepository = userBoardRepository;
            _currentUserService = currentUserService;
            _boardLabelRepository = boardLabelRepository;
            _localizer = localizer;
        }
        public async Task<Result<Guid>> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<Guid>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var boardWithSameKeyExists = await _boardRepository.Exists(request.Key);

            if(boardWithSameKeyExists)
            {
                return await Result<Guid>.FailAsync(_localizer["A board with the same key already exists. Pick a different key."]);
            }

            var board = new Board()
            {
                Title = request.Title,
                Description = request.Description,
                WorkspaceId = Guid.Parse(request.WorkspaceId),
                UserId = userId,
                Visibility = request.Visibility,
                Type = request.BoardType,
                Key = request.Key,
            };

            await _boardRepository.AddAsync(board);

            var userBoard = new UserBoard()
            {
                UserId = userId,
                BoardId = board.Id,
                Access = UserBoardAccess.Admin
            };

            var labels = new List<Label>();
            foreach (var colour in LabelColorsConstants.GetDefaultColors())
            {
                labels.Add(new Label()
                {
                    Colour = colour,
                    BoardId = board.Id
                });
            }

            board.Labels = labels;

            if(request.BoardType == BoardType.Scrum)
            {
                var boardLists = new List<BoardList>
                {
                    new BoardList()
                    {
                        Title = "TODO",
                        Position = 0,
                        UserId = userId,
                    },
                    new BoardList()
                    {
                        Title = "IN PROGRESS",
                        Position = 1,
                        UserId = userId,
                    },
                    new BoardList()
                    {
                        Title = "DONE",
                        Position = 2,
                        UserId = userId,
                    }
                };

                board.Lists = boardLists;
            }

            var dbResult = await _userBoardRepository.CreateAsync(userBoard);

            if (dbResult > 0)
            {
                return await Result<Guid>.SuccessAsync(board.Id, _localizer["Board Created"]);
            }

            return await Result<Guid>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
