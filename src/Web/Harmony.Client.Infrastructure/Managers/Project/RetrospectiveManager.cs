using Harmony.Application.DTO;
using Harmony.Application.Features.Retrospectives.Commands.Create;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    /// <summary>
    /// Client manager for Retrospectives
    /// </summary>
    public class RetrospectiveManager : IRetrospectiveManager
    {
        private readonly HttpClient _httpClient;

        public RetrospectiveManager(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<IResult<RetrospectiveDto>> Create(CreateRetrospectiveCommand command)
        {
            var response = await _httpClient
                .PostAsJsonAsync(Routes.RetrospectiveEndpoints.Index, command);

            return await response.ToResult<RetrospectiveDto>();
        }
    }
}
