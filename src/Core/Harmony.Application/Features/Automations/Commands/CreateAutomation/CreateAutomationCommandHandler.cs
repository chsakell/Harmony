using Harmony.Application.Contracts.Repositories;
using Harmony.Application.DTO.Automation;
using Harmony.Application.Features.Boards.Commands.AddUserBoard;
using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
            IAutomationDto automation = command.Type switch
            {
                AutomationType.SyncParentAndChildIssues 
                    => JsonSerializer.Deserialize<SyncParentAndChildIssuesAutomationDto>(command.Automation),
                    _ => throw new NotImplementedException("Automation not supported")
            };

            if(string.IsNullOrEmpty(automation.Id))
            {
                await _automationRepository.CreateAsync(automation);
                return await Result<string>.SuccessAsync(automation.Id, "Rule has been enabled");
            }
            else
            {
                var automationUpdated = await _automationRepository.UpdateAsync(automation);

                if(automationUpdated)
                {
                    return await Result<string>.SuccessAsync(automation.Id, "Rule has been updated");
                }

                return await Result<string>.SuccessAsync(automation.Id, "Failed to update rule");
            }
            
        }
    }
}
