using Harmony.Domain.Enums.Automations;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Automations.Commands.CreateAutomation
{
    public class CreateAutomationCommand : IRequest<Result<string>>
    {
        public string Automation { get; set; }
        public AutomationType Type { get; set; }

        public CreateAutomationCommand(string automation, AutomationType type)
        {
            Automation = automation;
            Type = type;
        }
    }
}
