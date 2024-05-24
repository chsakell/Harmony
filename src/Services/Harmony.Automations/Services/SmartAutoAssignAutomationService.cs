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
using Hangfire;
using Harmony.Domain.Enums.Automations;


namespace Harmony.Automations.Services
{
    public class SmartAutoAssignAutomationService : ISmartAutoAssignAutomationService
    {
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly UserCardService.UserCardServiceClient _userCardServiceClient;

        public SmartAutoAssignAutomationService(INotificationsPublisher notificationsPublisher,
            UserCardService.UserCardServiceClient userCardServiceClient)
        {
            _notificationsPublisher = notificationsPublisher;
            _userCardServiceClient = userCardServiceClient;
        }

        public async Task Process(SmartAutoAssignAutomationDto automation,
            CardCreatedMessage notification)
        {
            if (notification == null)
            {
                return;
            }

            var now = DateTime.Now;
            DateTime delay = now;

            switch (automation.RunTriggerAt)
            {
                case AutomationTriggerSchedule.Instantly:

                    break;
                case AutomationTriggerSchedule.After_5_Minutes:
                    delay = now.Add(TimeSpan.FromSeconds(5));
                    break;
                case AutomationTriggerSchedule.After_15_Minutes:
                    delay = now.Add(TimeSpan.FromMinutes(15));
                    break;
                case AutomationTriggerSchedule.After_30_Minutes:
                    delay = now.Add(TimeSpan.FromMinutes(30));
                    break;
                case AutomationTriggerSchedule.After_1_Hour:
                    delay = now.Add(TimeSpan.FromHours(1));
                    break;
                case AutomationTriggerSchedule.After_3_Hours:
                    delay = now.Add(TimeSpan.FromHours(3));
                    break;
                case AutomationTriggerSchedule.After_6_Hours:
                    delay = now.Add(TimeSpan.FromHours(6));
                    break;
                case AutomationTriggerSchedule.After_1_Day:
                    delay = now.Add(TimeSpan.FromDays(1));
                    break;
                default:
                    break;
            }

            var jobId = BackgroundJob
                .Schedule(() => ScheduleAssignee(automation, notification), delay);

            return;
        }

        public async Task ScheduleAssignee(SmartAutoAssignAutomationDto automation,
            CardCreatedMessage notification)
        {
            var userId = automation.Option == SmartAutoAssignOption.IssueCreator ?
                notification.UserId : automation.UserId;

            Metadata headers = new()
            {
                {
                    ServiceConstants.TrustedClientHeader,
                    ServiceConstants.HarmonyAutomations
                }
            };

            if (automation.AssignIfNoneAssigned)
            {
                var userAssignedResponse = await _userCardServiceClient.IsCardAssignedAsync(
                             new IsCardAssignedRequest
                             {
                                 CardId = notification.Card.Id.ToString(),
                             }, headers);

                if (userAssignedResponse.IsAssigned)
                {
                    return;
                }
            }

            if (automation.SetFromParentIfSubtask && notification.Card.ParentCardId.HasValue)
            {
                var userAssignedResponse = await _userCardServiceClient.IsCardAssignedAsync(
                             new IsCardAssignedRequest
                             {
                                 CardId = notification.Card.ParentCardId.Value.ToString(),
                             }, headers);

                if (userAssignedResponse.IsAssigned)
                {
                    userId = userAssignedResponse.Users[0];
                }
            }

            await _userCardServiceClient.AddUserCardAsync(
                              new AddUserCardRequest
                              {
                                  BoardId = notification.BoardId.ToString(),
                                  CardId = notification.Card.Id.ToString(),
                                  UserId = userId
                              }, headers);

        }
    }
}
