using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Automations.Commands.RemoveAutomation
{
    public class RemoveAutomationCommand : IRequest<Result<bool>>
    {
        public RemoveAutomationCommand(string automationId)
        {
            AutomationId = automationId;
        }

        public string AutomationId { get; set; }
    }
}
