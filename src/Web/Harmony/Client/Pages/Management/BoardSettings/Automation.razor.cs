using Harmony.Application.DTO.Automation;
using Microsoft.AspNetCore.Components;

namespace Harmony.Client.Pages.Management.BoardSettings
{
    public partial class Automation
    {
        [Parameter]
        public string Id { get; set; }
        private bool _drawerOpened = false;
        private string? _selectedTemplateId = null;

        private List<AutomationTemplateDto> _templates = new List<AutomationTemplateDto>();

        protected override async Task OnInitializedAsync()
        {
            var result = await _automationManager.GetTemplates();

            if (result.Succeeded)
            {
                _templates = result.Data;
            }
        }

        private void OpenTemplate(string templateId)
        {
            if(_drawerOpened)
            {
                if (_selectedTemplateId == templateId)
                {
                    _drawerOpened = false;
                    _selectedTemplateId = null;
                    return;
                }
            }
            else
            {
                _drawerOpened = true;
            }
            

            _selectedTemplateId = templateId;
        }
    }
}
