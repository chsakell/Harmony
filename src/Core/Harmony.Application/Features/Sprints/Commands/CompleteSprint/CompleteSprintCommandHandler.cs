using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Domain.Enums;
using AutoMapper;
using Harmony.Application.Features.Cards.Commands.MoveToBacklog;
using Harmony.Application.Features.Boards.Commands.CreateSprint;
using Harmony.Application.Features.Cards.Commands.MoveToSprint;

namespace Harmony.Application.Features.Sprints.Commands.CompleteSprint
{
    /// <summary>
    /// Handler for Completeing sprints
    /// </summary>
    public class CompleteSprintCommandHandler : IRequestHandler<CompleteSprintCommand, Result<bool>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ISprintRepository _sprintRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IBoardRepository _boardRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IStringLocalizer<CompleteSprintCommandHandler> _localizer;

        public CompleteSprintCommandHandler(ICurrentUserService currentUserService,
            ISprintRepository sprintRepository,
            IMapper mapper,
            IMediator mediator,
            IBoardRepository boardRepository,
            ICardRepository cardRepository,
            IStringLocalizer<CompleteSprintCommandHandler> localizer)
        {
            _currentUserService = currentUserService;
            _sprintRepository = sprintRepository;
            _mapper = mapper;
            _mediator = mediator;
            _boardRepository = boardRepository;
            _cardRepository = cardRepository;
            _localizer = localizer;
        }
        public async Task<Result<bool>> Handle(CompleteSprintCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            IResult operationResult = null;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var board = await _boardRepository.GetBoardWithLists(request.BoardId);

            if(request.MoveToBacklog)
            {
                var pendingCards = await _cardRepository.GetPendingSprintCards(request.SprintId);
                var moveToBackLogCommand = new MoveToBacklogCommand(request.BoardId, 
                        pendingCards.Select(c => c.Id).ToList());

                operationResult = await _mediator.Send(moveToBackLogCommand);
            }
            else if (request.CreateNewSprint)
            {
                var totalSprints = await _sprintRepository.CountSprints(request.BoardId);

                var createNewSprintCommand = new CreateEditSprintCommand(request.BoardId)
                {
                    Name = $"{board.Key} - Sprint {totalSprints + 1}"
                };

                var createSprintResult = await _mediator.Send(createNewSprintCommand);

                if(createSprintResult.Succeeded)
                {
                    var sprintCreated = createSprintResult.Data;

                    operationResult = await MoveToSprint(board, request, sprintCreated.Id); ;
                }
                else
                {
                    return await Result<bool>.FailAsync(operationResult.Messages);
                }
            }
            else if (request.MoveToSprintId.HasValue)
            {
                operationResult = await MoveToSprint(board, request, request.MoveToSprintId.Value);
            }

            if (!operationResult.Succeeded)
            {
                return await Result<bool>.FailAsync(operationResult.Messages);
            }

            var sprint = await _sprintRepository.GetSprint(request.SprintId);

            sprint.Status = SprintStatus.Completed;

            var dbResult = await _sprintRepository.Update(sprint);

            if (dbResult > 0)
            {
                return await Result<bool>.SuccessAsync(true, _localizer["Sprint has been completed"]);
            }

            return await Result<bool>.FailAsync(_localizer["Operation failed"]);
        }

        private async Task<IResult> MoveToSprint(Board board, CompleteSprintCommand request, Guid moveToSprintId)
        {
            var pendingCards = await _cardRepository.GetPendingSprintCards(request.SprintId);

            var moveToList = board.Lists
                    .FirstOrDefault(l => l.CardStatus == BoardListCardStatus.TODO || l.CardStatus == BoardListCardStatus.IN_PROGRESS);

            if (moveToList == null)
            {
                return await Result<bool>.FailAsync("TODO or IN PROGRESS board lists not found in the new sprint");
            }

            var moveToSprintCommand = new MoveToSprintCommand(request.BoardId, moveToSprintId, 
                moveToList.Id, pendingCards.Select(c => c.Id).ToList());

            return await _mediator.Send(moveToSprintCommand);
        }
    }
}
