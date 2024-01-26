using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Automations.Commands.ToggleAutomation
{
    public class ToggleAutomationCommand : IRequest<Result<bool>>
    {
        public ToggleAutomationCommand(string automationId, bool enabled)
        {
            AutomationId = automationId;
            Enabled = enabled;
        }

        public string AutomationId { get; set; }
        public bool Enabled { get; set; }
    }
}
