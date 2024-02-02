using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Shared.Constants.Application;
using Harmony.Domain.Enums;
using Harmony.Application.DTO;
using AutoMapper;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Shared.Utilities;

namespace Harmony.Application.Features.Boards.Commands.Create
{
    /// <summary>
    /// Handler for creating boards
    /// </summary>
    public class CreateBoardCommandHandler : IRequestHandler<CreateBoardCommand, Result<BoardDto>>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IUserBoardRepository _userBoardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IBoardLabelRepository _boardLabelRepository;
        private readonly IStringLocalizer<CreateBoardCommandHandler> _localizer;

        public CreateBoardCommandHandler(IBoardRepository boardRepository,
            IUserBoardRepository userBoardRepository,
            ICurrentUserService currentUserService,
            IMapper mapper, INotificationsPublisher notificationsPublisher,
            IBoardLabelRepository boardLabelRepository,
            IStringLocalizer<CreateBoardCommandHandler> localizer)
        {
            _boardRepository = boardRepository;
            _userBoardRepository = userBoardRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _notificationsPublisher = notificationsPublisher;
            _boardLabelRepository = boardLabelRepository;
            _localizer = localizer;
        }
        public async Task<Result<BoardDto>> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<BoardDto>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var boardWithSameKeyExists = await _boardRepository
                .Exists(Guid.Parse(request.WorkspaceId), request.Key);

            if (boardWithSameKeyExists)
            {
                return await Result<BoardDto>.FailAsync(_localizer["A board with the same key already exists. Pick a different key."]);
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

            var issueTypes = new List<IssueType>();
            foreach (var issueType in IssueTypesConstants.GetDefaultIssueTypes())
            {
                issueTypes.Add(new IssueType()
                {
                    Summary = issueType
                });
            }

            board.IssueTypes = issueTypes;

            var boardLists = new List<BoardList>
                {
                    new BoardList()
                    {
                        Title = "TODO",
                        Position = 0,
                        UserId = userId,
                        CardStatus = BoardListCardStatus.TODO
                    },
                    new BoardList()
                    {
                        Title = "IN PROGRESS",
                        Position = 1,
                        UserId = userId,
                        CardStatus= BoardListCardStatus.IN_PROGRESS
                    },
                    new BoardList()
                    {
                        Title = "DONE",
                        Position = 2,
                        UserId = userId,
                        CardStatus = BoardListCardStatus.DONE
                    }
                };

            board.Lists = boardLists;

            var dbResult = await _userBoardRepository.CreateAsync(userBoard);

            if (dbResult > 0)
            {
                await _boardRepository.LoadWorkspaceEntryAsync(board);
                var indexName = StringUtilities.SlugifyString($"{board.Workspace.Name}-{board.Title}");

                _notificationsPublisher
                    .PublishSearchIndexNotification(new BoardCreatedIndexNotification()
                    {
                        ObjectID = board.Id.ToString(),
                        SearchableAttributes = new List<string> 
                        { 
                            "title",
                            "description",
                            "serialKey", 
                            "listId", 
                            "issueType", 
                            "status",
                            "comments",
                            "members",
                            "hasAttachments",
                            "dueDate"
                        },
                        AttributesForFaceting = new List<string>
                        {
                            "hasAttachments"
                        }
                    }, indexName);

                var result = _mapper.Map<BoardDto>(board);

                return await Result<BoardDto>.SuccessAsync(result, _localizer["Board created"]);
            }

            return await Result<BoardDto>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
