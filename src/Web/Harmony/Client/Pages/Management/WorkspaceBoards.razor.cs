using Microsoft.AspNetCore.Components;

namespace Harmony.Client.Pages.Management
{
    public partial class WorkspaceBoards
    {
        [Parameter]
        public string Id { get; set; }

        [Parameter]
        public string Name { get; set; }
    }
}
