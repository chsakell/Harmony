using Harmony.Application.Features.Workspaces.Commands.Create;
using Harmony.Application.Features.Workspaces.Queries.GetAll;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using MediatR;
using System.Collections.Generic;
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

        public async Task<IResult<List<GetUserOwnedWorkspacesResponse>>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(Routes.WorkspaceEndpoints.Index);
            return await response.ToResult<List<GetUserOwnedWorkspacesResponse>>();
        }
    }
}
