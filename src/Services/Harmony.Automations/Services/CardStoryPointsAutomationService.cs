using Harmony.Application.Contracts.Automation;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.DTO.Automation;
using Harmony.Application.Notifications;
using Harmony.Automations.Contracts;
using Harmony.Domain.Enums;
using Harmony.Domain.Enums.Automations;

namespace Harmony.Automations.Services
{
    public class CardStoryPointsChangedAutomationService : IAutomationService<CardStoryPointsChangedMessage>
    {
        private readonly IAutomationRepository _automationRepository;
        private readonly ISumUpStoryPointsAutomationService _sumUpStoryPointsAutomationService;

        public CardStoryPointsChangedAutomationService(IAutomationRepository automationRepository,
            ISumUpStoryPointsAutomationService sumUpStoryPointsAutomationService)
        {
            _automationRepository = automationRepository;
            _sumUpStoryPointsAutomationService = sumUpStoryPointsAutomationService;
        }

        public async Task Run(CardStoryPointsChangedMessage notification)
        {
            var templatesForAutomation = await _automationRepository
                .GetTemplates(NotificationType.CardStoryPointsChanged);

            if(templatesForAutomation.Any())
            {
                var automationTypes = templatesForAutomation
                    .Select(template => template.Type).ToList();

                foreach(var automationType in automationTypes)
                {
                    switch(automationType)
                    {
                        case AutomationType.SumUpStoryPoints:
                            var automations = await _automationRepository
                                .GetAutomations<SumUpStoryPointsAutomationDto>
                                (AutomationType.SumUpStoryPoints, notification.BoardId);

                            foreach(var automation in automations)
                            {
                                await _sumUpStoryPointsAutomationService.Process(automation, notification);
                            }
                            break;
                    }
                    
                }
                
            }
        }
    }
}
