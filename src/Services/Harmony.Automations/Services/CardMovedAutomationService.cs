using Harmony.Application.Contracts.Automation;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.DTO.Automation;
using Harmony.Application.Notifications;
using Harmony.Automations.Contracts;
using Harmony.Domain.Enums;
using Harmony.Domain.Enums.Automations;

namespace Harmony.Automations.Services
{
    public class CardMovedAutomationService : IAutomationService<CardMovedMessage>
    {
        private readonly IAutomationRepository _automationRepository;
        private readonly ISyncParentAndChildIssuesAutomationService _syncParentAndChildIssuesAutomationService;

        public CardMovedAutomationService(IAutomationRepository automationRepository,
            ISyncParentAndChildIssuesAutomationService syncParentAndChildIssuesAutomationService)
        {
            _automationRepository = automationRepository;
            _syncParentAndChildIssuesAutomationService = syncParentAndChildIssuesAutomationService;
        }

        public async Task Run(CardMovedMessage notification)
        {
            var templatesForAutomation = await _automationRepository
                .GetTemplates(NotificationType.CardMoved);

            if(templatesForAutomation.Any())
            {
                var automationTypes = templatesForAutomation
                    .Select(template => template.Type).ToList();

                foreach(var automationType in automationTypes)
                {
                    switch(automationType)
                    {
                        case AutomationType.SyncParentAndChildIssues:
                            var automations = await _automationRepository
                                .GetAutomations<SyncParentAndChildIssuesAutomationDto>
                                (AutomationType.SyncParentAndChildIssues, notification.BoardId);

                            foreach(var automation in automations)
                            {
                                await _syncParentAndChildIssuesAutomationService.Process(automation, notification);
                            }
                            break;
                    }
                    
                }
                
            }
        }
    }
}
