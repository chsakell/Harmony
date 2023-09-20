using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Boards.Commands.CreateCard;
using Harmony.Application.Features.Boards.Commands.CreateList;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetAllForUser;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using MediatR;
using System.Collections.Generic;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public class CardManager : ICardManager
    {
        private readonly HttpClient _httpClient;

        public CardManager(HttpClient client)
        {
            _httpClient = client;
        }

		public async Task<IResult<CardDto>> CreateCardAsync(CreateCardCommand request)
		{
			var response = await _httpClient.PostAsJsonAsync(Routes.CardEndpoints.Index, request);

			return await response.ToResult<CardDto>();
		}
    }
}
