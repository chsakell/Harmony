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
using MongoDB.Driver;


namespace Harmony.Automations.Services
{
    public class SumUpStoryPointsAutomationService : ISumUpStoryPointsAutomationService
    {
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly CardService.CardServiceClient _cardServiceClient;

        public SumUpStoryPointsAutomationService(INotificationsPublisher notificationsPublisher,
            CardService.CardServiceClient cardServiceClient)
        {
            _notificationsPublisher = notificationsPublisher;
            _cardServiceClient = cardServiceClient;
        }

        public async Task Process(SumUpStoryPointsAutomationDto automation, CardStoryPointsChangedMessage notification)
        {
            if (notification == null || !notification.ParentCardId.HasValue)
            {
                return;
            }

            var parentCardResponse = await _cardServiceClient.GetCardAsync(
                              new CardFilterRequest { 
                                  CardId = notification.ParentCardId.ToString(),
                                  Children = true
                              });

            if (!parentCardResponse.Found)
            {
                return;
            }

            var parentCard = parentCardResponse.Card;

            if (automation.IssueTypes.Any() && !automation.IssueTypes.Contains(parentCard.IssueType))
            {
                return;
            }

            var syncStoryPointsResponse = await _cardServiceClient
                .SyncParentStoryPointsAsync(new SyncParentCardStoryPointsRequest()
                {
                    CardId = parentCard.CardId.ToString()
                });

            if(syncStoryPointsResponse.Success) 
            {
                var message = new
                    CardStoryPointsChangedMessage(notification.BoardId, 
                    Guid.Parse(parentCard.CardId),
                    (short?)syncStoryPointsResponse.TotalStoryPoints, null);

                _notificationsPublisher.PublishMessage(message,
                    NotificationType.CardStoryPointsChanged, 
                    routingKey: BrokerConstants.RoutingKeys.SignalR);
            }

            return;
        }
    }
}
