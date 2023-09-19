﻿using Harmony.Application.Features.Boards.Commands.Create;
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

		public async Task<IResult<Guid>> CreateListAsync(CreateListCommand request)
		{
			var response = await _httpClient.PostAsJsonAsync(Routes.BoardEndpoints.CreateList(request.BoardId), request);
			return await response.ToResult<Guid>();
		}

		public async Task<IResult<GetBoardResponse>> GetBoardAsync(string boardId)
        {
            var response = await _httpClient.GetAsync(Routes.BoardEndpoints.Get(boardId));
            return await response.ToResult<GetBoardResponse>();
        }

        public async Task<IResult<List<GetAllForUserBoardResponse>>> GetUserBoardsAsync()
        {
            var response = await _httpClient.GetAsync(Routes.BoardEndpoints.Index);
            return await response.ToResult<List<GetAllForUserBoardResponse>>();
        }
    }
}
