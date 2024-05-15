using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using AutoMapper;
using Harmony.Application.Features.Cards.Commands.CreateCheckListItem;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Domain.Enums;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications;
using Harmony.Application.DTO.Summaries;
using Harmony.Domain.Extensions;
using System.Text.Json;

namespace Harmony.Application.Features.Cards.Commands.CreateChecklist
{
    public class CreateCheckListItemCommandHandler : IRequestHandler<CreateCheckListItemCommand, Result<CheckListItemDto>>
    {
        private readonly ICheckListItemRepository _checkListItemRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICheckListRepository _checklistRepository;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IStringLocalizer<CreateChecklistCommandHandler> _localizer;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;

        public CreateCheckListItemCommandHandler(ICheckListItemRepository checkListItemRepository,
            ICurrentUserService currentUserService,
            ICheckListRepository checklistRepository,
            INotificationsPublisher notificationsPublisher,
            IStringLocalizer<CreateChecklistCommandHandler> localizer,
            ICacheService cacheService,
            IMapper mapper)
        {
            _checkListItemRepository = checkListItemRepository;
            _currentUserService = currentUserService;
            _checklistRepository = checklistRepository;
            _notificationsPublisher = notificationsPublisher;
            _localizer = localizer;
            _cacheService = cacheService;
            _mapper = mapper;
        }
        public async Task<Result<CheckListItemDto>> Handle(CreateCheckListItemCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<CheckListItemDto>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var items = await _checkListItemRepository.GetItems(request.CheckListId);

            var newItem = new CheckListItem()
            {
                CheckListId = request.CheckListId,
                Description = request.Description,
                Position = (byte)items.Count,
                DueDate = request.DueDate,
            };

            var dbResult = await _checkListItemRepository.CreateAsync(newItem);

            if (dbResult > 0)
            {
                var checkList = await _checklistRepository.Get(request.CheckListId);

                var cardSummary = await _cacheService.HashGetAsync<CardSummary>(
                        CacheKeys.ActiveCardSummaries(request.BoardId),
                        request.CardId.ToString());

                var cardSummaryCheckList = cardSummary.CheckLists
                    .FirstOrDefault(c => c.CheckListId == request.CheckListId);

                if (cardSummaryCheckList != null)
                {
                    cardSummaryCheckList.TotalItems += 1;

                    await _cacheService.HashHSetAsync(CacheKeys.ActiveCardSummaries(request.BoardId),
                    request.CardId.ToString(), JsonSerializer.Serialize(cardSummary, CacheDomainExtensions._jsonSerializerOptions));
                }

                var result = _mapper.Map<CheckListItemDto>(newItem);

                var message = new CardItemAddedMessage(request.BoardId, request.CardId);

                _notificationsPublisher.PublishMessage(message,
                    NotificationType.CardItemAdded, routingKey: BrokerConstants.RoutingKeys.SignalR);

                return await Result<CheckListItemDto>.SuccessAsync(result, _localizer["Item Created"]);
            }

            return await Result<CheckListItemDto>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
