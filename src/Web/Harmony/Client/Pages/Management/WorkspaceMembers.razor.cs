
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

        private List<UserWorkspaceResponse> _members;
        private MudTable<UserWorkspaceResponse> _table;
        private string _searchString = "";
        private int _totalItems;

        private bool _loading;

        //protected override async Task OnInitializedAsync()
        //{
        //    await GetUsers();
        //}

        //private async Task GetUsers()
        //{
        //    _loading = true;

        //    var workspaceMembersResult = await _workspaceManager.GetWorkspaceMembers(Id);

        //    if (workspaceMembersResult.Succeeded)
        //    {
        //        _members = workspaceMembersResult.Data;
        //    }

        //    _loading = false;
        //}

        //private async Task ReloadUsers()
        //{
        //    await GetUsers();
        //}

        private async Task<TableData<UserWorkspaceResponse>> ReloadData(TableState state)
        {
            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                state.Page = 0;
            }
            await LoadData(state.Page, state.PageSize, state);

            return new TableData<UserWorkspaceResponse> 
            { 
                TotalItems = _totalItems, 
                Items = _members
            };
        }

        private void OnSearch(string text)
        {
            _searchString = text;
            _table.ReloadServerData();
        }

        private async Task LoadData(int pageNumber, int pageSize, TableState state)
        {
            string[] orderings = null;
            if (!string.IsNullOrEmpty(state.SortLabel))
            {
                orderings = state.SortDirection != SortDirection.None ? new[] { $"{state.SortLabel} {state.SortDirection}" } : new[] { $"{state.SortLabel}" };
            }

            var request = new GetWorkspaceUsersQuery(Guid.Parse(Id))
            { 
                WorkspaceId = Guid.Parse(Id),
                PageSize = pageSize, 
                PageNumber = pageNumber + 1, 
                SearchTerm = _searchString, 
                OrderBy = orderings 
            };

            var response = await _workspaceManager.GetWorkspaceMembers(request);
            if (response.Succeeded)
            {
                _totalItems = response.TotalCount;
                _members = response.Data;
            }
            else
            {
                foreach (var message in response.Messages)
                {
                    _snackBar.Add(message, Severity.Error);
                }
            }
        }

        //private bool Search(UserWorkspaceResponse user)
        //{
        //    if (string.IsNullOrWhiteSpace(_searchString)) return true;
        //    if (user.FirstName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
        //    {
        //        return true;
        //    }
        //    if (user.LastName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
        //    {
        //        return true;
        //    }
        //    if (user.Email?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
        //    {
        //        return true;
        //    }
        //    if (user.UserName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
        //    {
        //        return true;
        //    }
        //    return false;
        //}
    }
}
