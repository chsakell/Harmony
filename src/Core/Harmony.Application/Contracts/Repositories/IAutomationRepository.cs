using Harmony.Application.DTO.Automation;
using Harmony.Application.Features.Automations.Commands.CreateAutomation;
using Harmony.Domain.Automation;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Repositories
{
    public interface IAutomationRepository
    {
        Task CreateTemplate(AutomationTemplate template);
        Task CreateTemplates(List<AutomationTemplate> templates);
        Task<List<AutomationTemplate>> GetTemplates();
        Task CreateAsync(IAutomationDto automation);
        Task<IEnumerable<T>> GetAutomations<T>(AutomationType type, Guid boardId);
        Task<IEnumerable<IAutomationDto>> GetAutomations(List<AutomationType> types, Guid boardId);
        Task<bool> ReplaceAsync(IAutomationDto automation);
        Task<bool> ChangeStatusAsync(string automationId, bool enabled);
        Task<bool> Remove(string automationId);
        Task<List<AutomationTemplate>> GetTemplates(NotificationType notificationType);
    }
}
