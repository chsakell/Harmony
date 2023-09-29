using Harmony.Application.Features.Workspaces.Commands.Create;
using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
using Harmony.Application.Features.Workspaces.Queries.LoadWorkspace;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Client.Infrastructure.Managers.Preferences;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public class WorkspaceManager : IWorkspaceManager
    {
        private readonly HttpClient _httpClient;
        private readonly IClientPreferenceManager _clientPreferenceManager;

        public List<GetAllForUserWorkspaceResponse> UserWorkspaces { get; private set; } = new List<GetAllForUserWorkspaceResponse>();
        public GetAllForUserWorkspaceResponse SelectedWorkspace { get; private set; }

        public WorkspaceManager(HttpClient client, ClientPreferenceManager clientPreferenceManager)
        {
            _httpClient = client;
            _clientPreferenceManager = clientPreferenceManager;
        }

        public async Task<IResult> CreateAsync(CreateWorkspaceCommand request)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.WorkspaceEndpoints.Index, request);
            return await response.ToResult();
        }

        public async Task<IResult<List<GetAllForUserWorkspaceResponse>>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(Routes.WorkspaceEndpoints.Index);
            return await response.ToResult<List<GetAllForUserWorkspaceResponse>>();
        }

        public async Task<IResult<List<LoadWorkspaceResponse>>> LoadWorkspaceAsync(string workspaceId)
        {
            var response = await _httpClient.GetAsync(Routes.WorkspaceEndpoints.Get(workspaceId));
            return await response.ToResult<List<LoadWorkspaceResponse>>();
        }

        public async Task InitAsync()
        {
            var workspacesResult = await GetAllAsync();

            if (workspacesResult.Succeeded)
            {
                UserWorkspaces = workspacesResult.Data;

                if (UserWorkspaces.Any())
                {
                    var selectedWorkspacePreference = await _clientPreferenceManager.GetSelectedWorkspace();

                    if (!string.IsNullOrEmpty(selectedWorkspacePreference))
                    {
                        SelectedWorkspace = UserWorkspaces
                            .FirstOrDefault(w => w.Id.ToString().Equals(selectedWorkspacePreference));
                    }
                    else
                    {
                        SelectedWorkspace = UserWorkspaces.First();
                        await _clientPreferenceManager.SetSelectedWorkspace(SelectedWorkspace.Id);
                    }

                    //if (SelectedWorkspace != null)
                    //{
                    //    Navigate(_selectedWorkspace);
                    //}
                }
            }
        }
    }
}
