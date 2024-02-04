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
            

            return;
        }
    }
}
