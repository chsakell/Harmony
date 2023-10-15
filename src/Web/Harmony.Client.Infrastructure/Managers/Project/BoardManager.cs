using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Features.Boards.Commands.AddUserBoard;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Boards.Commands.RemoveUserBoard;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Application.Features.Boards.Queries.SearchBoardUsers;
using Harmony.Application.Features.Cards.Commands.CreateCard;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public class BoardManager : IBoardManager
    {
        private readonly HttpClient _httpClient;
        public event EventHandler<BoardCreatedEvent> OnBoardCreated;

        public BoardManager(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<IResult<Guid>> CreateAsync(CreateBoardCommand request)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.BoardEndpoints.Index, request);

            var result = await response.ToResult<Guid>();

            if (result.Succeeded)
            {
                OnBoardCreated?.Invoke(this, new BoardCreatedEvent(request.WorkspaceId,
                    result.Data, request.Title, request.Description, request.Visibility));
            }

            return result;
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

        public async Task<IResult<List<UserBoardResponse>>> GetBoardMembersAsync(string boardId)
        {
            var response = await _httpClient.GetAsync(Routes.BoardEndpoints.GetMembers(boardId));

            return await response.ToResult<List<UserBoardResponse>>();
        }

        public async Task<IResult<List<SearchBoardUserResponse>>> SearchBoardMembersAsync(string boardId, string term)
        {
            var response = await _httpClient.GetAsync(Routes.BoardEndpoints.SearchMembers(boardId, term));

            return await response.ToResult<List<SearchBoardUserResponse>>();
        }

        public async Task<IResult<UserBoardResponse>> AddBoardMemberAsync(AddUserBoardCommand command)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.BoardEndpoints
                .GetMembers(command.BoardId.ToString()), command);

            return await response.ToResult<UserBoardResponse>();
        }

        public async Task<IResult<RemoveUserBoardResponse>> RemoveBoardMemberAsync(RemoveUserBoardCommand command)
        {
            var response = await _httpClient.DeleteAsync(Routes.BoardEndpoints
                .RemoveMember(command.BoardId.ToString(), command.UserId));

            return await response.ToResult<RemoveUserBoardResponse>();
        }
    }
}
