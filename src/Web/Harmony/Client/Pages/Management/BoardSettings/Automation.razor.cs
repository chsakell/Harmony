using Harmony.Application.DTO.Automation;
using Microsoft.AspNetCore.Components;

namespace Harmony.Client.Pages.Management.BoardSettings
{
    public partial class Automation
    {
        [Parameter]
        public string Id { get; set; }

        private List<AutomationTemplateDto> _templates = new List<AutomationTemplateDto>();

        protected override async Task OnInitializedAsync()
        {
            var result = await _automationManager.GetTemplates();

            if (result.Succeeded)
            {
                _templates = result.Data;
            }
        }
    }
}
