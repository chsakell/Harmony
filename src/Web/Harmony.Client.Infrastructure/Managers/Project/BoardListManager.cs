using Harmony.Application.DTO;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;
using Harmony.Application.Features.Lists.Commands.ArchiveList;
using Harmony.Application.Features.Lists.Commands.CreateList;
using Harmony.Application.Features.Lists.Commands.UpdateListTitle;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    /// <summary>
    /// Client manager for board lists
    /// </summary>
    public class BoardListManager : IBoardListManager
    {
        private readonly HttpClient _httpClient;

        public BoardListManager(HttpClient client)
        {
            _httpClient = client;
        }


		public async Task<IResult<BoardListDto>> CreateListAsync(CreateListCommand request)
		{
			var response = await _httpClient.PostAsJsonAsync(Routes.ListEndpoints.Index, request);

			return await response.ToResult<BoardListDto>();
		}


        public async Task<IResult<bool>> UpdateListStatusAsync(UpdateListStatusCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.ListEndpoints
                .GetListStatus(request.ListId), request);

            return await response.ToResult<bool>();
        }

        public async Task<IResult<bool>> UpdateListTitleAsync(UpdateListStatusCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.ListEndpoints
                .GetListStatus(request.ListId), request);

            return await response.ToResult<bool>();
        }

        public async Task<IResult<UpdateListTitleResponse>> UpdateBoardListTitleAsync(UpdateListTitleCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.BoardListEndpoints
                .GetListTitle(request.ListId), request);

            return await response.ToResult<UpdateListTitleResponse>();
        }
    }
}
