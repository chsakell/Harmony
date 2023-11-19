using Harmony.Application.Features.Sprints.StartSprint;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    /// <summary>
    /// Client manager for Sprints
    /// </summary>
    public class SprintManager : ISprintManager
    {
        private readonly HttpClient _httpClient;

        public SprintManager(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<IResult> StartSprint(StartSprintCommand request)
        {
            var response = await _httpClient
                .PutAsJsonAsync(Routes.SprintEndpoints.Start(request.SprintId), request);

            return await response.ToResult<bool>();
        }
    }
}
