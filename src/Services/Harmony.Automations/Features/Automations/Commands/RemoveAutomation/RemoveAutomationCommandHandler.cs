using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Automations.Commands.RemoveAutomation
{
    internal class RemoveAutomationCommandHandler
        : IRequestHandler<RemoveAutomationCommand, Result<bool>>
    {
        private readonly IAutomationRepository _automationRepository;

        public RemoveAutomationCommandHandler(IAutomationRepository automationRepository)
        {
            _automationRepository = automationRepository;
        }

        public async Task<Result<bool>> Handle(RemoveAutomationCommand command, CancellationToken cancellationToken)
        {
            var automationRemoved = await _automationRepository.Remove(command.AutomationId);

            if (automationRemoved)
            {
                return await Result<bool>.SuccessAsync(true, $"Rule has been removed");
            }

            return await Result<bool>.FailAsync("Failed to remove rule");
        }
    }
}
