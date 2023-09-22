using Harmony.Application.Features.Workspaces.Commands.Create;
using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
using Harmony.Application.Features.Workspaces.Queries.LoadWorkspace;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public class WorkspaceManager : IWorkspaceManager
    {
        private readonly HttpClient _httpClient;

        public WorkspaceManager(HttpClient client)
        {
            _httpClient = client;
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
    }
}
