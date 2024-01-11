using Harmony.Domain.Enums;

namespace Harmony.Application.Models
{
    public abstract class BaseAutomationCommand : BaseBoardCommand
    {
        public AutomationType AutomationType { get; set; }
    }
}
