using Harmony.Application.DTO;
using Harmony.Application.Features.Workspaces.Commands.Create;
using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceBoards;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
using Harmony.Application.Features.Workspaces.Queries.LoadWorkspace;
using Harmony.Application.Responses;
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

        public List<WorkspaceDto> UserWorkspaces { get; private set; } = new List<WorkspaceDto>();

        private WorkspaceDto _selectedWorkspace;
        public WorkspaceDto SelectedWorkspace
        {
            get
            {
                return _selectedWorkspace;
            }
            set
            {
                _selectedWorkspace = value;
                OnSelectedWorkspace?.Invoke(this, _selectedWorkspace);
            }
        }

        public event EventHandler<WorkspaceDto> OnSelectedWorkspace;

        public WorkspaceManager(HttpClient client, ClientPreferenceManager clientPreferenceManager)
        {
            _httpClient = client;
            _clientPreferenceManager = clientPreferenceManager;
        }

        public async Task<IResult<Guid>> CreateAsync(CreateWorkspaceCommand request)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.WorkspaceEndpoints.Index, request);

            var result = await response.ToResult<Guid>();

            if(result.Succeeded)
            {
                UserWorkspaces.Add(new WorkspaceDto()
                {
                    Id = result.Data,
                    Name = request.Name,
                    Description = request.Description
                });
            }
            
            return await response.ToResult<Guid>();
        }

        public async Task<IResult<List<WorkspaceDto>>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(Routes.WorkspaceEndpoints.Index);
            return await response.ToResult<List<WorkspaceDto>>();
        }

        public async Task<IResult<List<LoadWorkspaceResponse>>> LoadWorkspaceAsync(string workspaceId)
        {
            var response = await _httpClient.GetAsync(Routes.WorkspaceEndpoints.Get(workspaceId));
            return await response.ToResult<List<LoadWorkspaceResponse>>();
        }

        public async Task<IResult<List<GetWorkspaceBoardResponse>>> GetWorkspaceBoards(string workspaceId)
        {
            var response = await _httpClient.GetAsync(Routes.WorkspaceEndpoints.GetBoards(workspaceId));
            return await response.ToResult<List<GetWorkspaceBoardResponse>>();
        }

        public async Task<IResult<List<UserWorkspaceResponse>>> GetWorkspaceMembers(string workspaceId)
        {
            var response = await _httpClient.GetAsync(Routes.WorkspaceEndpoints.GetMembers(workspaceId));
            return await response.ToResult<List<UserWorkspaceResponse>>();
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
                            .FirstOrDefault(w => w.Id.ToString()
                            .Equals(selectedWorkspacePreference));
                    }
                    else
                    {
                        SelectedWorkspace = UserWorkspaces.First();
                        await _clientPreferenceManager.SetSelectedWorkspace(_selectedWorkspace.Id);
                    }
                }
            }
        }

        public async Task SelectWorkspace(Guid id)
        {
            var workspace = UserWorkspaces.FirstOrDefault(w => w.Id == id);

            if(workspace != null)
            {
                SelectedWorkspace = workspace;
                await _clientPreferenceManager.SetSelectedWorkspace(_selectedWorkspace.Id);
            }
        }
    }
}
