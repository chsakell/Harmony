using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Commands.CreateChecklist;
using Harmony.Application.Features.Cards.Commands.CreateCheckListItem;
using Harmony.Application.Features.Cards.Commands.UpdateCardTitle;
using Harmony.Application.Features.Lists.Commands.UpdateListItemDescription;
using Harmony.Application.Features.Lists.Commands.UpdateListTitle;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public class CheckListItemManager : ICheckListItemManager
    {
        private readonly HttpClient _httpClient;

        public CheckListItemManager(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<IResult<bool>> UpdateListItemDescriptionAsync(UpdateListItemDescriptionCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.CheckListItemEndpoints
                .Description(request.ListItemId), request);

            return await response.ToResult<bool>();
        }
    }
}
