using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Features.Boards.Commands.AddUserBoard;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Boards.Commands.RemoveUserBoard;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Application.Features.Boards.Queries.SearchBoardUsers;
using Harmony.Application.Features.Workspaces.Queries.SearchWorkspaceUsers;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IBoardManager : IManager
    {
        Task<IResult<Guid>> CreateAsync(CreateBoardCommand request);
        Task<IResult<GetBoardResponse>> GetBoardAsync(string boardId);
        event EventHandler<BoardCreatedEvent> OnBoardCreated;
        Task<IResult<List<UserBoardResponse>>> GetBoardMembersAsync(string boardId);
        Task<IResult<List<SearchBoardUserResponse>>> SearchBoardMembersAsync(string boardId, string term);
        Task<IResult<UserBoardResponse>> AddBoardMemberAsync(AddUserBoardCommand command);
        Task<IResult<RemoveUserBoardResponse>> RemoveBoardMemberAsync(RemoveUserBoardCommand command);
    }
}
