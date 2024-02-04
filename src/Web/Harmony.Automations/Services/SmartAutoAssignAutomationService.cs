using Harmony.Application.Contracts.Automation;
using Harmony.Application.DTO.Automation;
using Harmony.Application.Notifications;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Constants;
using Grpc.Net.Client;
using Harmony.Api.Protos;
using Microsoft.Extensions.Options;
using Harmony.Application.Configurations;
using Grpc.Core;


namespace Harmony.Automations.Services
{
    public class SmartAutoAssignAutomationService : ISmartAutoAssignAutomationService
    {
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly AppEndpointConfiguration _endpointConfiguration;

        public SmartAutoAssignAutomationService(INotificationsPublisher notificationsPublisher,
            IOptions<AppEndpointConfiguration> endpointsConfiguration)
        {
            _notificationsPublisher = notificationsPublisher;
            _endpointConfiguration = endpointsConfiguration.Value;
        }

        public async Task Process(SmartAutoAssignAutomationDto automation, CardCreatedMessage notification)
        {
            if (notification == null)
            {
                return;
            }

            switch (automation.Option)
            {
                case Domain.Enums.Automations.SmartAutoAssignOption.IssueCreator:
                    await SetCardAssignee(notification.BoardId, notification.Card.Id, notification.UserId);
                    break;
                case Domain.Enums.Automations.SmartAutoAssignOption.SpecificUser:

                    break;
            }

            return;
        }

        private async Task SetCardAssignee(Guid boardId, Guid cardId, string userId)
        {
            using var channel = GrpcChannel.ForAddress(_endpointConfiguration.HarmonyApiEndpoint);
            var client = new UserCardService.UserCardServiceClient(channel);

            Metadata headers = new()
            {
                { ServiceConstants.TrustedClientHeader, ServiceConstants.HarmonyAutomations }
            };

            var cardResponse = await client.AddUserCardAsync(
                              new AddUserCardRequest
                              {
                                  BoardId = boardId.ToString(),
                                  CardId = cardId.ToString(),
                                  UserId = userId
                              }, headers);

        }
    }
}
