using Harmony.Application.Features.Boards.Commands;
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
    }
}
