namespace Harmony.Client.Shared.Components
{
    public partial class WorkspaceSelector
    {
        protected override async Task OnInitializedAsync()
        {
            await _workspaceManager.GetAllAsync();
        }
    }
}
