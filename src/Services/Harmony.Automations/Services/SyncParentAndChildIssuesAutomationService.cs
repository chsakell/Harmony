using Harmony.Application.Contracts.Automation;
using Harmony.Application.DTO.Automation;
using Harmony.Application.Notifications;
using Microsoft.EntityFrameworkCore;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Constants;
using Harmony.Domain.Enums;
using Grpc.Net.Client;
using Harmony.Api.Protos;
using Microsoft.Extensions.Options;
using Harmony.Application.Configurations;


namespace Harmony.Automations.Services
{
    public class SyncParentAndChildIssuesAutomationService : ISyncParentAndChildIssuesAutomationService
    {
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly CardService.CardServiceClient _cardServiceClient;

        public SyncParentAndChildIssuesAutomationService(INotificationsPublisher notificationsPublisher,
            CardService.CardServiceClient cardServiceClient)
        {
            _notificationsPublisher = notificationsPublisher;
            _cardServiceClient = cardServiceClient;
        }

        public async Task Process(SyncParentAndChildIssuesAutomationDto automation, CardMovedMessage notification)
        {
            if (notification == null || !notification.ParentCardId.HasValue)
            {
                return;
            }

            var cardResponse = await _cardServiceClient.GetCardAsync(
                              new CardFilterRequest { 
                                  CardId = notification.ParentCardId.ToString(),
                                  Children = true
                              });

            if (!cardResponse.Found)
            {
                return;
            }

            var card = cardResponse.Card;

            if (notification.MovedToListId.ToString() != card.BoardListId)
            {
                // check all children have the same status
                var allChildrenHaveSameStatus = card.Children.All(c => c.BoardListId == notification.MovedToListId.ToString());

                var currentStatusConditionPass = !automation.FromStatuses.Any() ||
                    automation.FromStatuses.Contains(card.BoardListId.ToString());

                var destinationStatusConditionPass = !automation.ToStatuses.Any() ||
                    automation.ToStatuses.Contains(notification.MovedToListId.ToString());

                if (allChildrenHaveSameStatus && currentStatusConditionPass && destinationStatusConditionPass)
                {
                    var currentBoardListId = card.BoardListId;

                    var updateResult = await _cardServiceClient.MoveCardToListAsync(
                              new MoveCardToListRequest
                              {
                                  CardId = notification.ParentCardId.ToString(),
                                  BoardListId = notification.MovedToListId.ToString()
                              });

                    if (updateResult.Success)
                    {
                        var cardMovedNotification = new CardMovedMessage()
                        {
                            BoardId = notification.BoardId,
                            CardId = Guid.Parse(card.CardId),
                            FromPosition = (short)card.Position,
                            ToPosition = (short)updateResult.NewPosition,
                            MovedFromListId = Guid.Parse(card.BoardListId),
                            MovedToListId = notification.MovedToListId,
                            IsCompleted = notification.IsCompleted,
                            UpdateId = notification.UpdateId ?? Guid.NewGuid(),
                        };

                        _notificationsPublisher.PublishMessage(cardMovedNotification,
                            NotificationType.CardMoved, routingKey: BrokerConstants.RoutingKeys.SignalR);
                    }
                }
            }

            return;
        }
    }
}
