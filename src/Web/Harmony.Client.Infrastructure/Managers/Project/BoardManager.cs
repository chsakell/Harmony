using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Boards.Commands.CreateList;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Cards.Commands.CreateCard;
using Harmony.Application.Features.Lists.Commands.ArchiveList;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using MediatR;
using System.Collections.Generic;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public class BoardManager : IBoardManager
    {
        private readonly HttpClient _httpClient;

        public BoardManager(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<IResult> CreateAsync(CreateBoardCommand request)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.BoardEndpoints.Index, request);
            return await response.ToResult();
        }

		public async Task<IResult<CardDto>> CreateCardAsync(CreateCardCommand request)
		{
			var response = await _httpClient.PostAsJsonAsync(Routes.BoardEndpoints
                .CreateCard(request.BoardId, request.ListId), request);

			return await response.ToResult<CardDto>();
		}

		public async Task<IResult<GetBoardResponse>> GetBoardAsync(string boardId)
        {
            var response = await _httpClient.GetAsync(Routes.BoardEndpoints.Get(boardId));
            return await response.ToResult<GetBoardResponse>();
        }
    }
}
