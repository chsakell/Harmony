using Harmony.Application.Contracts.Repositories;
using Harmony.Application.DTO.Automation;
using Harmony.Domain.Enums.Automations;
using Harmony.Shared.Wrapper;
using MediatR;
using System.Text.Json;

namespace Harmony.Application.Features.Automations.Commands.CreateAutomation
{
    internal class CreateAutomationCommandHandler
        : IRequestHandler<CreateAutomationCommand, Result<string>>
    {
        private readonly IAutomationRepository _automationRepository;

        public CreateAutomationCommandHandler(IAutomationRepository automationRepository)
        {
            _automationRepository = automationRepository;
        }

        public async Task<Result<string>> Handle(CreateAutomationCommand command, CancellationToken cancellationToken)
        {
            IAutomationDto? automation = command.Type switch
            {
                AutomationType.SyncParentAndChildIssues 
                    => JsonSerializer.Deserialize<SyncParentAndChildIssuesAutomationDto>(command.Automation),
                AutomationType.SmartAutoAssign
                => JsonSerializer.Deserialize<SmartAutoAssignAutomationDto>(command.Automation),
                _ => null
            };

            if(automation == null)
            {
                return Result<string>.Fail("Failed to serialize automation");
            }

            if(string.IsNullOrEmpty(automation.Id))
            {
                await _automationRepository.CreateAsync(automation);
                return await Result<string>.SuccessAsync(automation.Id, "Rule has been enabled");
            }
            else
            {
                var automationUpdated = await _automationRepository.ReplaceAsync(automation);

                if(automationUpdated)
                {
                    return await Result<string>.SuccessAsync(automation.Id, "Rule has been updated");
                }

                return await Result<string>.SuccessAsync(automation.Id, "Failed to update rule");
            }
            
        }
    }
}
