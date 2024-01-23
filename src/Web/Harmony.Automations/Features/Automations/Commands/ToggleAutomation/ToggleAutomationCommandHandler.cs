using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Automations.Commands.ToggleAutomation
{
    internal class ToggleAutomationCommandHandler
        : IRequestHandler<ToggleAutomationCommand, Result<bool>>
    {
        private readonly IAutomationRepository _automationRepository;

        public ToggleAutomationCommandHandler(IAutomationRepository automationRepository)
        {
            _automationRepository = automationRepository;
        }

        public async Task<Result<bool>> Handle(ToggleAutomationCommand command, CancellationToken cancellationToken)
        {
            var automationUpdated = await _automationRepository.ChangeStatusAsync(command.AutomationId, command.Enabled);

            if (automationUpdated)
            {
                return await Result<bool>.SuccessAsync(true, $"Rule has been {(command.Enabled ? "enabled" : "disabled")}");
            }

            return await Result<bool>.FailAsync("Failed to update rule");
        }
    }
}
