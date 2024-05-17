using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;
using Harmony.Application.Contracts.Messaging;
using Harmony.Domain.Enums;
using Harmony.Application.Notifications;
using Harmony.Application.Constants;
using Harmony.Application.DTO.Summaries;
using Harmony.Domain.Extensions;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Harmony.Application.Contracts.Services.Caching;

namespace Harmony.Application.Features.Cards.Commands.DeleteChecklist
{
    public class DeleteChecklistCommandHandler : IRequestHandler<DeleteCheckListCommand, Result<bool>>
    {
        private readonly ICheckListRepository _checklistRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IStringLocalizer<DeleteChecklistCommandHandler> _localizer;
        private readonly ICardSummaryService _cardSummaryService;
        private readonly IMapper _mapper;

        public DeleteChecklistCommandHandler(ICheckListRepository checklistRepository,
            ICurrentUserService currentUserService,
            INotificationsPublisher notificationsPublisher,
            IStringLocalizer<DeleteChecklistCommandHandler> localizer,
            ICardSummaryService cardSummaryService,
            IMapper mapper)
        {
            _checklistRepository = checklistRepository;
            _currentUserService = currentUserService;
            _notificationsPublisher = notificationsPublisher;
            _localizer = localizer;
            _cardSummaryService = cardSummaryService;
            _mapper = mapper;
        }
        public async Task<Result<bool>> Handle(DeleteCheckListCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var checkList = await _checklistRepository.GetWithItems(request.CheckListId);

            if (checkList == null)
            {
                return await Result<bool>.FailAsync(_localizer["Check List doesn't exist"]);
            }

            var totalItems = checkList.Items.Count;
            var totalItemsCompleted = checkList.Items.Where(i => i.IsChecked).Count();

            var boardId = await _checklistRepository.GetBoardId(checkList.Id);

            var dbResult = await _checklistRepository.Delete(checkList);
            if (dbResult > 0)
            {
                await _cardSummaryService.UpdateCardSummary(request.BoardId, checkList.CardId,
                (summary) =>
                {
                    var cardSummaryCheckList = summary.CheckLists
                        .FirstOrDefault(c => c.CheckListId == request.CheckListId);

                    if (cardSummaryCheckList != null)
                    {
                        summary.CheckLists.Remove(cardSummaryCheckList);
                    }
                });

                var message = new CheckListRemovedMessage(boardId, checkList.Id, checkList.CardId, totalItems, totalItemsCompleted);

                _notificationsPublisher.PublishMessage(message,
                    NotificationType.CheckListRemoved, routingKey: BrokerConstants.RoutingKeys.SignalR);

                return await Result<bool>.SuccessAsync(true, _localizer["Check List deleted successfully"]);
            }

            return await Result<bool>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
