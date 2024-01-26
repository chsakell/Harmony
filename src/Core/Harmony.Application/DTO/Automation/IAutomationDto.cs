using Harmony.Domain.Enums;

namespace Harmony.Application.DTO.Automation
{
    public interface IAutomationDto
    {
        string Id { get; set; }
        AutomationType Type { get; set; }
        string BoardId { get; set; }
        bool Enabled { get; set; }
    }
}
