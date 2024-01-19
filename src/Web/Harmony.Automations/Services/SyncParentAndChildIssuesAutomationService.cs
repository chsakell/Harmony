using Harmony.Application.Contracts.Automation;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.DTO.Automation;
using Harmony.Application.Notifications;
using Harmony.Application.Specifications.Cards;
using Harmony.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Constants;
using Harmony.Domain.Enums;

namespace Harmony.Automations.Services
{
    public class SyncParentAndChildIssuesAutomationService : ISyncParentAndChildIssuesAutomationService
    {
        private readonly ICardRepository _cardRepository;
        private readonly INotificationsPublisher _notificationsPublisher;

        public SyncParentAndChildIssuesAutomationService(ICardRepository cardRepository, 
            INotificationsPublisher notificationsPublisher)
        {
            _cardRepository = cardRepository;
            _notificationsPublisher = notificationsPublisher;
        }

        public async Task Process(SyncParentAndChildIssuesAutomationDto automation, CardMovedMessage notification)
        {
            if(notification == null || !notification.ParentCardId.HasValue)
            {
                return;
            }

            var includes = new CardIncludes() { Children = true };

            var filter = new CardFilterSpecification(notification.ParentCardId, includes);

            var card = await _cardRepository
                .Entities.IgnoreQueryFilters().Specify(filter)
                .FirstOrDefaultAsync();

            if(card == null)
            {
                return;
            }

            if(notification.MovedToListId != card.BoardListId)
            {
                // check all children have the same status
                var allChildrenHaveSameStatus = card.Children.All(c => c.BoardListId == notification.MovedToListId);

                var currentStatusConditionPass = !automation.FromStatuses.Any() ||
                    automation.FromStatuses.Contains(card.BoardListId.ToString());

                var destinationStatusConditionPass = !automation.ToStatuses.Any() ||
                    automation.ToStatuses.Contains(notification.MovedToListId.ToString());

                if (allChildrenHaveSameStatus && currentStatusConditionPass && destinationStatusConditionPass)
                {
                    var currentBoardListId = card.BoardListId;
                    card.BoardListId = notification.MovedToListId;

                    // make this the last card in the list
                    var totalCards = await _cardRepository.CountCards(notification.MovedToListId.Value);
                    card.Position = (short)totalCards;

                    var updateResult = await _cardRepository.Update(card);

                    var cardMovedNotification = new CardMovedMessage()
                    {
                        BoardId = notification.BoardId,
                        CardId = card.Id,
                        FromPosition = card.Position,
                        ToPosition = card.Position,
                        MovedFromListId = currentBoardListId,
                        MovedToListId = card.BoardListId,
                        IsCompleted = notification.IsCompleted,
                        UpdateId = notification.UpdateId ?? Guid.NewGuid(),
                    };

                    _notificationsPublisher.PublishMessage(cardMovedNotification,
                        NotificationType.CardMoved, routingKey: BrokerConstants.RoutingKeys.SignalR);
                }
            }

            return;
        }
    }
}
