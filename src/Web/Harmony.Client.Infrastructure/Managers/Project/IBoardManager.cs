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
using Harmony.Application.Features.Cards.Commands.MoveToBacklog;
using Harmony.Application.Features.Cards.Commands.MoveToSprint;
using Harmony.Application.Features.Cards.Commands.ReactivateCards;
using Harmony.Application.Features.Lists.Commands.UpdateListsPositions;
using Harmony.Application.Features.Lists.Queries.GetBoardLists;
using Harmony.Application.Features.Lists.Queries.LoadBoardList;
using Harmony.Application.Features.Workspaces.Queries.GetBacklog;
using Harmony.Application.Features.Workspaces.Queries.GetSprints;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IBoardManager : IManager
    {
        Task<IResult<BoardDto>> CreateAsync(CreateBoardCommand request);
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
        Task<PaginatedResult<GetSprintCardResponse>> GetSprintCards(GetSprintCardsQuery request);
        Task<PaginatedResult<SprintDto>> GetSprints(GetSprintsQuery request);
        Task<IResult<SprintDto>> CreateSprintAsync(CreateEditSprintCommand request);
        Task<IResult<List<GetBoardListResponse>>> GetBoardListsAsync(string boardId);
        Task<IResult<List<CardDto>>> MoveCardsToSprint(MoveToSprintCommand request);
        Task<IResult<List<CardDto>>> ReactivateCards(ReactivateCardsCommand request);
        Task<IResult<List<CardDto>>> MoveCardsToBacklog(MoveToBacklogCommand request);
        Task<IResult<GetPendingSprintCardsResponse>> GetPendingSprintCards(GetPendingSprintCardsQuery request);
        Task<IResult<List<BoardDto>>> GetUserBoards();
        Task<PaginatedResult<GetArchivedItemResponse>> GetArchivedItems(GetArchivedItemsQuery request);
    }
}
