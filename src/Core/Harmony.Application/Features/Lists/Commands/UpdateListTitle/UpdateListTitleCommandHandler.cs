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

namespace Harmony.Application.Features.Lists.Commands.UpdateListTitle
{
    public class UpdateListTitleCommandHandler : IRequestHandler<UpdateListTitleCommand, Result<UpdateListTitleResponse>>
    {
        private readonly IBoardListRepository _boardListRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IStringLocalizer<UpdateListTitleCommandHandler> _localizer;
        private readonly IMapper _mapper;

        public UpdateListTitleCommandHandler(IBoardListRepository ListRepository,
            ICurrentUserService currentUserService,
            INotificationsPublisher notificationsPublisher,
            IStringLocalizer<UpdateListTitleCommandHandler> localizer,
            IMapper mapper)
        {
            _boardListRepository = ListRepository;
            _currentUserService = currentUserService;
            _notificationsPublisher = notificationsPublisher;
            _localizer = localizer;
            _mapper = mapper;
        }
        public async Task<Result<UpdateListTitleResponse>> Handle(UpdateListTitleCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<UpdateListTitleResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var list = await _boardListRepository.Get(request.ListId);

            list.Title = request.Title;

            var dbResult = await _boardListRepository.Update(list);

            if (dbResult > 0)
            {
                var result = new UpdateListTitleResponse(request.BoardId, list.Id, list.Title);
                
                var message = new BoardListTitleChangedMessage(request.BoardId, list.Id, list.Title);

                _notificationsPublisher.PublishMessage(message,
                    NotificationType.BoardListTitleChanged, routingKey: BrokerConstants.RoutingKeys.SignalR);

                return await Result<UpdateListTitleResponse>.SuccessAsync(result, _localizer["List title updated"]);
            }

            return await Result<UpdateListTitleResponse>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
