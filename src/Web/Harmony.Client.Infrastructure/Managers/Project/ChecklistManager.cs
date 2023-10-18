using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Features.Cards.Commands.CreateChecklist;
using Harmony.Application.Features.Cards.Commands.CreateCheckListItem;
using Harmony.Application.Features.Lists.Commands.UpdateListTitle;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public class ChecklistManager : IChecklistManager
    {
        private readonly HttpClient _httpClient;

        public ChecklistManager(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<IResult<CheckListDto>> CreateCheckListAsync(CreateCheckListCommand request)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.CheckListEndpoints.Index, request);

            return await response.ToResult<CheckListDto>();
        }

        public async Task<IResult<CheckListItemDto>> CreateCheckListItemAsync(CreateCheckListItemCommand request)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.CheckListEndpoints.GetListItems(request.CheckListId), request);

            return await response.ToResult<CheckListItemDto>();
        }

        public async Task<IResult<bool>> UpdateTitleAsync(UpdateListTitleCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.CheckListEndpoints.Title(request.ListId), request);

            return await response.ToResult<bool>();
        }
    }
}
