using Harmony.Application.Features.Labels.Commands.CreateCardLabel;
using Harmony.Application.Features.Labels.Commands.RemoveCardLabel;
using Harmony.Application.Features.Labels.Commands.UpdateTitle;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    /// <summary>
    /// Client manager for labels
    /// </summary>
    public class LabelManager : ILabelManager
    {
        private readonly HttpClient _httpClient;

        public LabelManager(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<IResult<CreateCardLabelResponse>> CreateCardLabel(CreateCardLabelCommand request)
        {
            var response = await _httpClient
                .PostAsJsonAsync(Routes.LabelEndpoints.Index, request);

            return await response.ToResult<CreateCardLabelResponse>();
        }

        public async Task<IResult<bool>> RemoveCardLabel(RemoveCardLabelCommand request)
        {
            var response = await _httpClient.DeleteAsync(Routes.LabelEndpoints
                .GetLabel(request.LabelId));

            return await response.ToResult<bool>();
        }

        public async Task<IResult> UpdateLabelTitle(UpdateLabelTitleCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.LabelEndpoints.LabelTitle(request.LabelId), request);

            return await response.ToResult<bool>();
        }
    }
}
