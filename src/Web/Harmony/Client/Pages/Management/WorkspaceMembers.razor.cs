
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
using Harmony.Application.Responses;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Pages.Management
{
    public partial class WorkspaceMembers
    {
        [Parameter]
        public string Id { get; set; }

        [Parameter]
        public string Name { get; set; }

        private List<UserWorkspaceResponse> _members = new();
        private UserWorkspaceResponse _user = new();
        private string _searchString = "";

        private bool _loading;

        protected override async Task OnInitializedAsync()
        {
            await GetUsers();
        }

        private async Task GetUsers()
        {
            _loading = true;

            var workspaceMembersResult = await _workspaceManager.GetWorkspaceMembers(Id);

            if (workspaceMembersResult.Succeeded)
            {
                _members = workspaceMembersResult.Data;
            }

            _loading = false;
        }

        private async Task ReloadUsers()
        {
            await GetUsers();
        }

        private bool Search(UserResponse user)
        {
            if (string.IsNullOrWhiteSpace(_searchString)) return true;
            if (user.FirstName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }
            if (user.LastName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }
            if (user.Email?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }
            if (user.UserName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }
            return false;
        }
    }
}
