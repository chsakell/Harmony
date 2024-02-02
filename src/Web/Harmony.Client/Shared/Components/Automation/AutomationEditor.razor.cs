using Harmony.Domain.Enums.Automations;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Components.Automation
{
    public partial class AutomationEditor
    {
        [Parameter]
        public bool IsOpened { get; set; }

        [Parameter]
        public Guid BoardId { get; set; }

        [Parameter]
        public AutomationType? AutomationType { get; set; }

        [Parameter]
        public string TemplateName { get; set; }

        [Parameter]
        public EventCallback OnClose { get; set; }

        private MudDrawer _drawer;

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        protected override Task OnParametersSetAsync()
        {
            return base.OnParametersSetAsync();
        }

        public async Task Close()
        {
            await OnClose.InvokeAsync();
        }
    }
}
