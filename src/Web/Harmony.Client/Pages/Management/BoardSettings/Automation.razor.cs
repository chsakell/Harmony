using Harmony.Application.DTO.Automation;
using Harmony.Domain.Enums.Automations;
using Microsoft.AspNetCore.Components;

namespace Harmony.Client.Pages.Management.BoardSettings
{
    public partial class Automation
    {
        [Parameter]
        public string Id { get; set; }
        private bool _drawerOpened = false;
        private string _selectedTemplateName = string.Empty;
        private AutomationType? _automationType = null;

        private List<AutomationTemplateDto> _templates = new List<AutomationTemplateDto>();

        protected override async Task OnParametersSetAsync()
        {
            var result = await _automationManager.GetTemplates();

            if (result.Succeeded)
            {
                _templates = result.Data;
            }
        }

        private void OpenTemplate(AutomationTemplateDto template)
        {
            if(_drawerOpened)
            {
                if (_automationType == template.Type)
                {
                    _drawerOpened = false;
                    _automationType = null;
                    return;
                }
            }
            else
            {
                _drawerOpened = true;
            }

            _automationType = template.Type;
            _selectedTemplateName = template.Name;
        }
    }
}
