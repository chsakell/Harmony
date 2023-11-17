using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Features.Boards.Commands.AddUserBoard;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Boards.Commands.CreateSprint;
using Harmony.Application.Features.Boards.Commands.RemoveUserBoard;
using Harmony.Application.Features.Boards.Commands.UpdateUserBoardAccess;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetBacklog;
using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Features.Boards.Queries.SearchBoardUsers;
using Harmony.Application.Features.Lists.Commands.UpdateListsPositions;
using Harmony.Application.Features.Lists.Queries.LoadBoardList;
using Harmony.Application.Features.Workspaces.Queries.GetBacklog;
using Harmony.Application.Features.Workspaces.Queries.GetSprints;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IBoardManager : IManager
    {
        Task<IResult<Guid>> CreateAsync(CreateBoardCommand request);
        Task<IResult<GetBoardResponse>> GetBoardAsync(string boardId, int size);
        event EventHandler<BoardCreatedEvent> OnBoardCreated;
        Task<IResult<List<UserBoardResponse>>> GetBoardMembersAsync(string boardId);
        Task<IResult<List<SearchBoardUserResponse>>> SearchBoardMembersAsync(string boardId, string term);
        Task<IResult<UserBoardResponse>> AddBoardMemberAsync(AddUserBoardCommand command);
        Task<IResult<RemoveUserBoardResponse>> RemoveBoardMemberAsync(RemoveUserBoardCommand command);
        Task<IResult<UpdateUserBoardAccessResponse>> UpdateBoardUserAccessAsync(UpdateUserBoardAccessCommand request);
        Task<IResult<UpdateListsPositionsResponse>> UpdateBoardListsPositions(UpdateListsPositionsCommand request);
        Task<IResult<List<CardDto>>> GetBoardListCardsAsync(LoadBoardListQuery request);
        Task<PaginatedResult<GetBacklogItemResponse>> GetBacklog(GetBacklogQuery request);
        Task<IResult<List<IssueTypeDto>>> GetIssueTypesAsync(string boardId);
        Task<PaginatedResult<GetSprintItemResponse>> GetSprints(GetSprintsQuery request);
        Task<IResult<SprintDto>> CreateSprintAsync(CreateSprintCommand request);
    }
}
