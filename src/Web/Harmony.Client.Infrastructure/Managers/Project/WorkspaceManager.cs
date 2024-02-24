using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Features.Workspaces.Commands.AddMember;
using Harmony.Application.Features.Workspaces.Commands.Create;
using Harmony.Application.Features.Workspaces.Commands.RemoveMember;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceBoards;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
using Harmony.Application.Features.Workspaces.Queries.LoadWorkspace;
using Harmony.Application.Features.Workspaces.Queries.SearchWorkspaceUsers;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Client.Infrastructure.Managers.Preferences;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using Polly;
using Polly.Registry;
using System.Net.Http.Json;
using static Harmony.Shared.Constants.Application.ApplicationConstants;
using static MudBlazor.CategoryTypes;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    /// <summary>
    /// Client manager for workspaces
    /// </summary>
    public class WorkspaceManager : IWorkspaceManager
    {
        private readonly HttpClient _httpClient;
        private readonly IClientPreferenceManager _clientPreferenceManager;

        private readonly ResiliencePipeline _resiliencePipeline;

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
        public event EventHandler<WorkspaceAddedEvent> OnWorkspaceAdded;

        public WorkspaceManager(HttpClient client, 
            ClientPreferenceManager clientPreferenceManager,
            ResiliencePipelineProvider<string> resiliencePipelineProvider)
        {
            _httpClient = client;
            _clientPreferenceManager = clientPreferenceManager;
            _resiliencePipeline = resiliencePipelineProvider.GetPipeline(HarmonyRetryPolicy.WaitAndRetry);
        }

        public async Task<IResult<WorkspaceDto>> CreateAsync(CreateWorkspaceCommand request)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.WorkspaceEndpoints.Index, request);

            var result = await response.ToResult<WorkspaceDto>();

            if(result.Succeeded)
            {

                UserWorkspaces.Add(result.Data);

                OnWorkspaceAdded?.Invoke(this, new WorkspaceAddedEvent(result.Data));
            }
            
            return result;
        }

        public async Task<IResult<List<WorkspaceDto>>> GetAllAsync()
        {
            return await _resiliencePipeline.ExecuteAsync(async token =>
            {
                var response = await _httpClient.GetAsync(Routes.WorkspaceEndpoints.Index);
                return await response.ToResult<List<WorkspaceDto>>();
            });
        }

        public async Task<IResult<List<BoardDto>>> LoadWorkspaceAsync(string workspaceId)
        {
            var response = await _httpClient.GetAsync(Routes.WorkspaceEndpoints.Get(workspaceId));
            return await response.ToResult<List<BoardDto>>();
        }

        public async Task<IResult<List<GetWorkspaceBoardResponse>>> GetWorkspaceBoards(string workspaceId)
        {
            var response = await _httpClient.GetAsync(Routes.WorkspaceEndpoints.GetBoards(workspaceId));
            return await response.ToResult<List<GetWorkspaceBoardResponse>>();
        }

        public async Task<PaginatedResult<UserWorkspaceResponse>> GetWorkspaceMembers(GetWorkspaceUsersQuery request)
        {
            var response = await _httpClient.GetAsync(Routes.WorkspaceEndpoints
                .GetMembers(request.WorkspaceId, request.PageNumber, request.PageSize,
                     request.SearchTerm, request.OrderBy, request.MembersOnly));

            return await response.ToPaginatedResult<UserWorkspaceResponse>();
        }

        public async Task<IResult<List<SearchWorkspaceUserResponse>>> SearchWorkspaceMembers(SearchWorkspaceUsersQuery request)
        {
            var response = await _httpClient.GetAsync(Routes.WorkspaceEndpoints
                .SearchMembers(request.WorkspaceId.ToString(), request.SearchTerm));

            return await response.ToResult<List<SearchWorkspaceUserResponse>>();
        }

        public async Task<IResult<bool>> AddWorkspaceMember(AddWorkspaceMemberCommand request)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.WorkspaceEndpoints
                .GetAddMembers(request.WorkspaceId.ToString()), request);

            return await response.ToResult<bool>();
        }

        public async Task<IResult<bool>> RemoveWorkspaceMember(RemoveWorkspaceMemberCommand request)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.WorkspaceEndpoints
                .GetRemoveMembers(request.WorkspaceId.ToString()), request);

            return await response.ToResult<bool>();
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

                    if (!string.IsNullOrEmpty(selectedWorkspacePreference) &&
                        UserWorkspaces
                            .Any(w => w.Id.ToString()
                            .Equals(selectedWorkspacePreference)))
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
