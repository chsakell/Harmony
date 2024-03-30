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
using Harmony.Application.Specifications.Cards;
using Harmony.Application.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Application.Features.Retrospectives.Commands.Create
{
    /// <summary>
    /// Handler for creating retrospectives
    /// </summary>
    public class CreateRetrospectiveCommandHandler : IRequestHandler<CreateRetrospectiveCommand, Result<RetrospectiveDto>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRetrospectiveRepository _retrospectiveRepository;
        private readonly IBoardRepository _boardRepository;
        private readonly IMapper _mapper;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IStringLocalizer<CreateRetrospectiveCommandHandler> _localizer;

        public CreateRetrospectiveCommandHandler(ICurrentUserService currentUserService,
            IRetrospectiveRepository retrospectiveRepository,
            IBoardRepository boardRepository,
            IMapper mapper, INotificationsPublisher notificationsPublisher,
            IStringLocalizer<CreateRetrospectiveCommandHandler> localizer)
        {
            _currentUserService = currentUserService;
            _retrospectiveRepository = retrospectiveRepository;
            _boardRepository = boardRepository;
            _mapper = mapper;
            _notificationsPublisher = notificationsPublisher;
            _localizer = localizer;
        }

        public async Task<Result<RetrospectiveDto>> Handle(CreateRetrospectiveCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<RetrospectiveDto>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            // check if sprint already has a retrospective
            if(request.SprintId.HasValue)
            {
                var filter = new RetrospectiveFilterSpecification(request.BoardId)
                {
                    SprintId = request.SprintId.Value
                };

                filter.Build();

                var sprintRetro = await _retrospectiveRepository
                    .Entities.Specify(filter)
                    .FirstOrDefaultAsync();

                if(sprintRetro != null)
                {
                    return await Result<RetrospectiveDto>.FailAsync($"The sprint already contains the '{sprintRetro.Name}' retrospective");
                }
            }

            var parentBoard = await _boardRepository.GetAsync(request.BoardId);

            var board = new Board()
            {
                Title = request.Name,
                Description = $"Retrospective's Board",
                WorkspaceId = parentBoard.WorkspaceId,
                UserId = userId,
                Visibility = BoardVisibility.Private,
                Type = BoardType.Retrospective,
                Key = parentBoard.Key + Random.Shared.Next(100),
            };

            var boardLists = new List<BoardList>
                {
                    new BoardList()
                    {
                        Title = "WENT WELL",
                        Position = 0,
                        UserId = userId
                    },
                    new BoardList()
                    {
                        Title = "TO IMPROVE",
                        Position = 1,
                        UserId = userId
                    },
                    new BoardList()
                    {
                        Title = "ACTION ITEMS",
                        Position = 2,
                        UserId = userId,
                        CardStatus = BoardListCardStatus.DONE
                    }
                };

            board.Lists = boardLists;

            await _boardRepository.AddAsync(board);

            var retrospective = new Retrospective()
            {
                Name = request.Name,
                ParentBoardId = request.BoardId,
                Board = board,
                BoardId = board.Id,
                SprintId = request.SprintId,
                DisableVotingInitially = request.DisableVotingInitially,
                HideCardsInitially = request.HideCardsInitially,
                HideVoteCount = request.HideVoteCount,
                MaxVotesPerUser = request.MaxVotesPerUser,
                ShowCardsAuthor = request.ShowCardsAuthor,
                Type = request.Type,
                UserId = userId
            };

            var dbResult = await _retrospectiveRepository.Create(retrospective);

            if (dbResult > 0)
            {

                var result = _mapper.Map<RetrospectiveDto>(retrospective);

                return await Result<RetrospectiveDto>.SuccessAsync(result, _localizer["Retrospective created"]);
            }

            return await Result<RetrospectiveDto>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
