using Harmony.Application.Features.Automations.Commands.CreateAutomation;
using Harmony.Domain.Automation;
using Harmony.Domain.Entities;
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
        Task<List<AutomationTemplate>> GetTemplates();
        Task<int> CreateAsync(CreateAutomationCommand automation);
    }
}
