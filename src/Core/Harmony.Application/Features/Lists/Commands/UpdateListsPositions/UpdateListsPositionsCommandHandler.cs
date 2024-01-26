using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Lists.Commands.UpdateListsPositions
{
    public class UpdateListsPositionsCommandHandler : IRequestHandler<UpdateListsPositionsCommand, Result<UpdateListsPositionsResponse>>
    {
        private readonly IBoardListRepository _boardListRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IStringLocalizer<CreateBoardCommandHandler> _localizer;
        private readonly IMapper _mapper;

        public UpdateListsPositionsCommandHandler(IBoardListRepository boardListRepository,
            ICurrentUserService currentUserService,
            INotificationsPublisher notificationsPublisher,
            IStringLocalizer<CreateBoardCommandHandler> localizer,
            IMapper mapper)
        {
            _boardListRepository = boardListRepository;
            _currentUserService = currentUserService;
            _notificationsPublisher = notificationsPublisher;
            _localizer = localizer;
            _mapper = mapper;
        }
        public async Task<Result<UpdateListsPositionsResponse>> Handle(UpdateListsPositionsCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<UpdateListsPositionsResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var boardLists = await _boardListRepository.GetBoardLists(request.BoardId);

            foreach (var list in boardLists)
            {
                var newPosition = request.ListPositions[list.Id];
                list.Position = newPosition;
            }

            var dbResult = await _boardListRepository.UpdateRange(boardLists);

            if (dbResult > 0)
            {
                var result = _mapper.Map<UpdateListsPositionsResponse>(request);

                var message = new BoardListsPositionsChangedMessage(request.BoardId, result.ListPositions);

                _notificationsPublisher.PublishMessage(message,
                    NotificationType.BoardListsPositionChanged, routingKey: BrokerConstants.RoutingKeys.SignalR);

                return await Result<UpdateListsPositionsResponse>.SuccessAsync(result, _localizer["List re ordered"]);
            }

            return await Result<UpdateListsPositionsResponse>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
