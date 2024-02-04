using Harmony.Application.Contracts.Automation;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.DTO.Automation;
using Harmony.Application.Notifications;
using Harmony.Automations.Contracts;
using Harmony.Domain.Enums;
using Harmony.Domain.Enums.Automations;

namespace Harmony.Automations.Services
{
    public class CardCreatedAutomationService : IAutomationService<CardCreatedMessage>
    {
        private readonly IAutomationRepository _automationRepository;
        private readonly ISmartAutoAssignAutomationService _smartAutoAssignAutomationService;

        public CardCreatedAutomationService(IAutomationRepository automationRepository,
            ISmartAutoAssignAutomationService smartAutoAssignAutomationService)
        {
            _automationRepository = automationRepository;
            _smartAutoAssignAutomationService = smartAutoAssignAutomationService;
        }

        public async Task Run(CardCreatedMessage notification)
        {
            var templatesForAutomation = await _automationRepository
                .GetTemplates(NotificationType.CardCreated);

            if(templatesForAutomation.Any())
            {
                var automationTypes = templatesForAutomation
                    .Select(template => template.Type).ToList();

                foreach(var automationType in automationTypes)
                {
                    switch(automationType)
                    {
                        case AutomationType.SmartAutoAssign:
                            var automations = await _automationRepository
                                .GetAutomations<SmartAutoAssignAutomationDto>
                                (AutomationType.SmartAutoAssign, notification.BoardId);

                            foreach(var automation in automations)
                            {
                                await _smartAutoAssignAutomationService.Process(automation, notification);
                            }
                            break;
                    }
                    
                }
                
            }
        }
    }
}
