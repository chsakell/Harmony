using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
using Harmony.Shared.Utilities;

namespace Harmony.Client.Shared.Components
{
    public partial class WorkspaceSelector
    {
        private void Navigate(GetAllForUserWorkspaceResponse workspace)
        {
            var slug = StringUtilities.SlugifyString(workspace.Name);
            _navigationManager.NavigateTo($"workspaces/{workspace.Id}/{slug}");
        }
    }
}
