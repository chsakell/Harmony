﻿using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
using Harmony.Shared.Utilities;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Components
{
    public partial class WorkspaceSelector
    {
        List<GetAllForUserWorkspaceResponse> _ownedWorkspaces = new List<GetAllForUserWorkspaceResponse>();
        GetAllForUserWorkspaceResponse? _selectedWorkspace;
        protected override async Task OnInitializedAsync()
        {
            var ownedWorkspacesResult = await _workspaceManager.GetAllAsync();
            
            if(ownedWorkspacesResult.Succeeded)
            {
                _ownedWorkspaces = ownedWorkspacesResult.Data;

                if (ownedWorkspacesResult.Data.Any())
                {
                    var selectedWorkspacePreference = await _clientPreferenceManager.GetSelectedWorkspace();

                    if (!string.IsNullOrEmpty(selectedWorkspacePreference))
                    {
                        _selectedWorkspace = _ownedWorkspaces
                            .FirstOrDefault(w => w.Id.ToString().Equals(selectedWorkspacePreference));
                    }
                    else
                    {
                        _selectedWorkspace = _ownedWorkspaces.First();
                        await _clientPreferenceManager.SetSelectedWorkspace(_selectedWorkspace.Id);
                    }

                    if (_selectedWorkspace != null)
                    {
                        Navigate(_selectedWorkspace);
                    }
                }
            }
        }

        private void Navigate(GetAllForUserWorkspaceResponse workspace)
        {
            var slug = StringUtilities.SlugifyString(workspace.Name);
            _navigationManager.NavigateTo($"workspaces/{workspace.Id}/{slug}");
        }
    }
}