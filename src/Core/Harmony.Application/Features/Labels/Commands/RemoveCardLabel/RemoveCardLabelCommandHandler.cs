using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications;
using Harmony.Domain.Enums;
using Harmony.Domain.Extensions;
using Harmony.Domain.Entities;

namespace Harmony.Application.Features.Labels.Commands.RemoveCardLabel
{
    public class RemoveCardLabelCommandHandler : IRequestHandler<RemoveCardLabelCommand, IResult<bool>>
    {
        private readonly IBoardLabelRepository _boardLabelRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly ICacheService _cacheService;
        private readonly IStringLocalizer<RemoveCardLabelCommandHandler> _localizer;
        private readonly IMapper _mapper;

        public RemoveCardLabelCommandHandler(IBoardLabelRepository boardLabelRepository,
            ICurrentUserService currentUserService,
            INotificationsPublisher notificationsPublisher,
            ICacheService cacheService,
            IStringLocalizer<RemoveCardLabelCommandHandler> localizer,
            IMapper mapper)
        {
            _boardLabelRepository = boardLabelRepository;
            _currentUserService = currentUserService;
            _notificationsPublisher = notificationsPublisher;
            _cacheService = cacheService;
            _localizer = localizer;
            _mapper = mapper;
        }
        public async Task<IResult<bool>> Handle(RemoveCardLabelCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var label = await _boardLabelRepository.GetLabel(request.LabelId);

            if (label == null)
            {
                return await Result<bool>.FailAsync(_localizer["Label not found"]);
            }

            var dbResult = await _boardLabelRepository.Delete(label);
            if (dbResult > 0)
            {
                var boardLabels = await _cacheService.HashGetAsync<List<Label>>(
                        CacheKeys.Board(label.BoardId),
                        CacheKeys.BoardLabels(label.BoardId));

                if (boardLabels.Any(l => l.Id == label.Id))
                {
                    var labelToRemove = boardLabels.FirstOrDefault(l => l.Id == label.Id);
                    boardLabels.Remove(labelToRemove);

                    await _cacheService.HashHSetAsync(CacheKeys.Board(label.BoardId),
                        CacheKeys.BoardLabels(label.BoardId),
                        CacheDomainExtensions.SerializeLabels(boardLabels));

                    await _cacheService.RemoveAsync(CacheKeys.ActiveCardSummaries(label.BoardId));
                }

                var message = new CardLabelRemovedMessage(label.BoardId, label.Id);

                _notificationsPublisher.PublishMessage(message,
                    NotificationType.CardLabelRemoved, routingKey: BrokerConstants.RoutingKeys.SignalR);

                return await Result<bool>.SuccessAsync(true, _localizer["Label removed"]);
            }

            return await Result<bool>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
