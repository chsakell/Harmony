using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications;
using Harmony.Domain.Enums;
using static Harmony.Application.Notifications.BoardListArchivedMessage;
using Harmony.Application.Models;
using Harmony.Domain.Entities;
using Harmony.Domain.Extensions;

namespace Harmony.Application.Features.Lists.Commands.ArchiveList
{
    public class ArchiveListCommandHandler : IRequestHandler<UpdateListStatusCommand, Result<bool>>
    {
        private readonly IBoardListRepository _boardListRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<ArchiveListCommandHandler> _localizer;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IListService _listService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;

        public ArchiveListCommandHandler(IBoardListRepository boardListRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<ArchiveListCommandHandler> localizer,
            INotificationsPublisher notificationsPublisher,
            IListService listService,
            ICacheService cacheService,
            IMapper mapper)
        {
            _boardListRepository = boardListRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _notificationsPublisher = notificationsPublisher;
            _listService = listService;
            _cacheService = cacheService;
            _mapper = mapper;
        }
        public async Task<Result<bool>> Handle(UpdateListStatusCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            List<BoardListOrder> newPositions = null;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var list = await _boardListRepository.Get(request.ListId);

            list.Status = request.Status;

            if(request.Status == BoardListStatus.Archived)
            {
                newPositions = await _listService.ReorderAfterArchive(list);
            }

            var dbResult = await _boardListRepository.Update(list);

            if (dbResult > 0)
            {
                if(request.Status == BoardListStatus.Archived)
                {
                    var lists = await _cacheService.HashGetAsync<List<BoardList>>(
                        CacheKeys.Board(request.BoardId), CacheKeys.BoardLists(request.BoardId));

                    var archivedList = lists.FirstOrDefault(l => l.Id == request.ListId);

                    if (archivedList != null)
                    {
                        lists.Remove(archivedList);
                        await _cacheService.HashHSetAsync(CacheKeys.Board(request.BoardId), 
                            CacheKeys.BoardLists(request.BoardId), lists.SerializeLists());
                    }

                    var message = new BoardListArchivedMessage(list.BoardId, list.Id, newPositions);

                    _notificationsPublisher.PublishMessage(message,
                        NotificationType.BoardListArchived, routingKey: BrokerConstants.RoutingKeys.SignalR);
                }

                return await Result<bool>.SuccessAsync(true, _localizer["List status updated"]);
            }

            return await Result<bool>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
