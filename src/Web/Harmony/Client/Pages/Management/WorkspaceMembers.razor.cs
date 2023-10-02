
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
using Harmony.Application.Responses;
using Harmony.Client.Shared.Dialogs;
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
        public bool _filterMembersOnly { get; set; } = true;

        private bool _loading;

       
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
                OrderBy = orderings,
                MembersOnly = _filterMembersOnly
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

        private async Task AddMember(UserWorkspaceResponse user)
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to add {user.UserName} as a member to the workspace?" },
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Success }
            };

            var result = _dialogService.Show<Confirmation>("Confirm", parameters);

            if (!result.Result.IsCanceled)
            {

            }

        }

        private async Task RemoveMember(UserWorkspaceResponse user)
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to remove {user.UserName} from the workspace?" },
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Error }
            };

            var result = _dialogService.Show<Confirmation>("Confirm", parameters);

            if (!result.Result.IsCanceled)
            {

            }
        }
    }
}
