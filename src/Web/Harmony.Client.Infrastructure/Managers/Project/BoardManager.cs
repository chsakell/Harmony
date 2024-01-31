using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Features.Boards.Commands.AddUserBoard;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Boards.Commands.CreateSprint;
using Harmony.Application.Features.Boards.Commands.RemoveUserBoard;
using Harmony.Application.Features.Boards.Commands.UpdateUserBoardAccess;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetArchivedItems;
using Harmony.Application.Features.Boards.Queries.GetBacklog;
using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Features.Boards.Queries.SearchBoardUsers;
using Harmony.Application.Features.Cards.Commands.CreateCard;
using Harmony.Application.Features.Cards.Commands.MoveToBacklog;
using Harmony.Application.Features.Cards.Commands.MoveToSprint;
using Harmony.Application.Features.Cards.Commands.ReactivateCards;
using Harmony.Application.Features.Lists.Commands.UpdateListsPositions;
using Harmony.Application.Features.Lists.Queries.GetBoardLists;
using Harmony.Application.Features.Lists.Queries.LoadBoardList;
using Harmony.Application.Features.Workspaces.Queries.GetBacklog;
using Harmony.Application.Features.Workspaces.Queries.GetSprints;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    /// <summary>
    /// Client manager for boards
    /// </summary>
    public class BoardManager : IBoardManager
    {
        private readonly HttpClient _httpClient;
        public event EventHandler<BoardCreatedEvent> OnBoardCreated;

        public BoardManager(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<IResult<BoardDto>> CreateAsync(CreateBoardCommand request)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.BoardEndpoints.Index, request);

            var result = await response.ToResult<BoardDto>();

            if (result.Succeeded)
            {
                var board = result.Data;

                OnBoardCreated?.Invoke(this, 
                    new BoardCreatedEvent(request.WorkspaceId, board));
            }

            return result;
        }

		public async Task<IResult<CardDto>> CreateCardAsync(CreateCardCommand request)
		{
			var response = await _httpClient.PostAsJsonAsync(Routes.BoardEndpoints
                .CreateCard(request.BoardId, request.ListId), request);

			return await response.ToResult<CardDto>();
		}

		public async Task<IResult<GetBoardResponse>> GetBoardAsync(string boardId, int size)
        {
            var response = await _httpClient.GetAsync(Routes.BoardEndpoints.Get(boardId, size));
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
                .Member(command.BoardId.ToString(), command.UserId));

            return await response.ToResult<RemoveUserBoardResponse>();
        }

        public async Task<IResult<UpdateUserBoardAccessResponse>> UpdateBoardUserAccessAsync(UpdateUserBoardAccessCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.BoardEndpoints
                .MemberStatus(request.BoardId.ToString(), request.UserId), request);

            return await response.ToResult<UpdateUserBoardAccessResponse>();
        }

        public async Task<IResult<UpdateListsPositionsResponse>> UpdateBoardListsPositions(UpdateListsPositionsCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.BoardEndpoints
                .BoardListPositions(request.BoardId.ToString()), request);

            return await response.ToResult<UpdateListsPositionsResponse>();
        }

        public async Task<IResult<List<CardDto>>> GetBoardListCardsAsync(LoadBoardListQuery request)
        {
            var response = await _httpClient
                .GetAsync(Routes.BoardEndpoints.GetBoardList(request.BoardId.ToString(), 
                    request.BoardListId, request.Page, request.PageSize));
            return await response.ToResult<List<CardDto>>();
        }

        public async Task<PaginatedResult<GetBacklogItemResponse>> GetBacklog(GetBacklogQuery request)
        {
            var response = await _httpClient.GetAsync(Routes.BoardEndpoints
                .Backlog(request.BoardId.ToString(), request.PageNumber, request.PageSize,
                     request.SearchTerm, request.OrderBy));

            return await response.ToPaginatedResult<GetBacklogItemResponse>();
        }

        public async Task<PaginatedResult<GetArchivedItemResponse>> GetArchivedItems(GetArchivedItemsQuery request)
        {
            var response = await _httpClient.GetAsync(Routes.BoardEndpoints
                .ArchivedItems(request.BoardId.ToString(), request.PageNumber, request.PageSize,
                     request.SearchTerm, request.OrderBy));

            return await response.ToPaginatedResult<GetArchivedItemResponse>();
        }

        public async Task<PaginatedResult<GetSprintCardResponse>> GetSprintCards(GetSprintCardsQuery request)
        {
            var response = await _httpClient.GetAsync(Routes.BoardEndpoints
                .SprintCards(request.BoardId.ToString(), request.PageNumber, request.PageSize,
                     request.SearchTerm, request.OrderBy, request.SprintStatus));

            return await response.ToPaginatedResult<GetSprintCardResponse>();
        }

        public async Task<IResult<GetPendingSprintCardsResponse>> GetPendingSprintCards(GetPendingSprintCardsQuery request)
        {
            var response = await _httpClient
                .GetAsync(Routes.BoardEndpoints
                .GetSprintPendingCards(request.BoardId, request.SprintId));

            return await response.ToResult<GetPendingSprintCardsResponse>();
        }

        public async Task<PaginatedResult<SprintDto>> GetSprints(GetSprintsQuery request)
        {
            var response = await _httpClient.GetAsync(Routes.BoardEndpoints
                .Sprints(request.BoardId.ToString(), request.PageNumber, request.PageSize,
                     request.SearchTerm, request.OrderBy, request.Statuses));

            return await response.ToPaginatedResult<SprintDto>();
        }

        public async Task<IResult<List<IssueTypeDto>>> GetIssueTypesAsync(string boardId)
        {
            var response = await _httpClient.GetAsync(Routes.BoardEndpoints.GetIssueTypes(boardId));

            return await response.ToResult<List<IssueTypeDto>>();
        }

        public async Task<IResult<SprintDto>> CreateSprintAsync(CreateEditSprintCommand request)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.BoardEndpoints
                .Sprints(request.BoardId), request);

            return await response.ToResult<SprintDto>();
        }

        public async Task<IResult<List<GetBoardListResponse>>> GetBoardListsAsync(string boardId)
        {
            var response = await _httpClient.GetAsync(Routes.BoardEndpoints.GetBoardLists(boardId));

            return await response.ToResult<List<GetBoardListResponse>>();
        }

        public async Task<IResult<List<CardDto>>> MoveCardsToSprint(MoveToSprintCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.BoardEndpoints
                .MoveCardsToSprint(request.BoardId.ToString()), request);

            return await response.ToResult<List<CardDto>>();
        }

        public async Task<IResult<List<CardDto>>> ReactivateCards(ReactivateCardsCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.BoardEndpoints
                .ReactivateCards(request.BoardId.ToString()), request);

            return await response.ToResult<List<CardDto>>();
        }

        public async Task<IResult<List<CardDto>>> MoveCardsToBacklog(MoveToBacklogCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.BoardEndpoints
                .MoveCardsToBacklog(request.BoardId.ToString()), request);

            return await response.ToResult<List<CardDto>>();
        }

        public async Task<IResult<List<BoardDto>>> GetUserBoards()
        {
            var response = await _httpClient.GetAsync(Routes.BoardEndpoints.UserBoards);
            return await response.ToResult<List<BoardDto>>();
        }
    }
}
