using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Commands.CreateCard;
using Harmony.Application.Features.Cards.Commands.MoveCard;
using Harmony.Application.Features.Cards.Commands.UpdateCardDescription;
using Harmony.Application.Features.Cards.Commands.UpdateCardTitle;
using Harmony.Application.Features.Cards.Queries.LoadCard;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
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

        public async Task<IResult<LoadCardResponse>> LoadCardAsync(LoadCardQuery request)
        {
            var response = await _httpClient.GetAsync(Routes.CardEndpoints.Get(request.CardId));

            return await response.ToResult<LoadCardResponse>();
        }


        public async Task<IResult<CardDto>> CreateCardAsync(CreateCardCommand request)
		{
			var response = await _httpClient.PostAsJsonAsync(Routes.CardEndpoints.Index, request);

			return await response.ToResult<CardDto>();
		}

		public async Task<IResult<CardDto>> MoveCardAsync(MoveCardCommand request)
		{
			var response = await _httpClient.PutAsJsonAsync(Routes.CardEndpoints.Move(request.CardId), request);

			return await response.ToResult<CardDto>();
		}

        public async Task<IResult<bool>> UpdateDescriptionAsync(UpdateCardDescriptionCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.CardEndpoints.Description(request.CardId), request);

            return await response.ToResult<bool>();
        }

        public async Task<IResult<bool>> UpdateTitleAsync(UpdateCardTitleCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.CardEndpoints.Title(request.CardId), request);

            return await response.ToResult<bool>();
        }
    }
}
