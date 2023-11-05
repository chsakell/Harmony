using Harmony.Application.Events;
using Harmony.Application.Features.Lists.Commands.UpdateListItemChecked;
using Harmony.Application.Features.Lists.Commands.UpdateListItemDescription;
using Harmony.Application.Features.Lists.Commands.UpdateListItemDueDate;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    /// <summary>
    /// Client manager for check lists items
    /// </summary>
    public class CheckListItemManager : ICheckListItemManager
    {
        private readonly HttpClient _httpClient;
        
        public event EventHandler<CardItemAddedEvent> OnCardItemAdded;

        public CheckListItemManager(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<IResult<bool>> UpdateListItemCheckedAsync(UpdateListItemCheckedCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.CheckListItemEndpoints
                .Checked(request.ListItemId), request);

            return await response.ToResult<bool>();
        }

        public async Task<IResult<bool>> UpdateListItemDescriptionAsync(UpdateListItemDescriptionCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.CheckListItemEndpoints
                .Description(request.ListItemId), request);

            return await response.ToResult<bool>();
        }

        public async Task<IResult<bool>> UpdateListItemDueDateAsync(UpdateListItemDueDateCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.CheckListItemEndpoints
                .DueDate(request.ListItemId), request);

            return await response.ToResult<bool>();
        }
    }
}
