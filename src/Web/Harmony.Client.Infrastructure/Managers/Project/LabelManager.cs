using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Features.Labels.Commands.UpdateTitle;
using Harmony.Application.Features.Lists.Commands.UpdateListTitle;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public class LabelManager : ILabelManager
    {
        private readonly HttpClient _httpClient;

        public LabelManager(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<IResult> UpdateLabelTitle(UpdateLabelTitleCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.LabelEndpoints.LabelTitle(request.LabelId), request);

            return await response.ToResult<bool>();
        }
    }
}
