using Harmony.Application.Features.Sprints.Commands.CompleteSprint;
using Harmony.Application.Features.Sprints.Commands.StartSprint;
using Harmony.Application.Features.Sprints.Queries.GetSprintReports;
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

        public async Task<IResult<bool>> CompleteSprint(CompleteSprintCommand request)
        {
            var response = await _httpClient
                .PostAsJsonAsync(Routes.SprintEndpoints.Complete(request.SprintId), request);

            return await response.ToResult<bool>();
        }

        public async Task<IResult<GetSprintReportsResponse>> GetSprintReports(Guid sprintId)
        {
            var response = await _httpClient
                .GetAsync(Routes.SprintEndpoints.Reports(sprintId));

            return await response.ToResult<GetSprintReportsResponse>();
        }
    }
}
