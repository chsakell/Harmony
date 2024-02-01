using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Features.Workspaces.Commands.AddMember;
using Harmony.Application.Features.Workspaces.Commands.Create;
using Harmony.Application.Features.Workspaces.Commands.RemoveMember;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceBoards;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
using Harmony.Application.Features.Workspaces.Queries.LoadWorkspace;
using Harmony.Application.Features.Workspaces.Queries.SearchWorkspaceUsers;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IWorkspaceManager : IManager
    {
        Task InitAsync();
        List<WorkspaceDto> UserWorkspaces { get; }
        WorkspaceDto SelectedWorkspace { get; }
        Task<IResult<WorkspaceDto>> CreateAsync(CreateWorkspaceCommand request);
        Task SelectWorkspace(Guid id);
        Task<IResult<List<WorkspaceDto>>> GetAllAsync();
        Task<IResult<List<BoardDto>>> LoadWorkspaceAsync(string workspaceId);
        Task<IResult<List<GetWorkspaceBoardResponse>>> GetWorkspaceBoards(string workspaceId);
        Task<PaginatedResult<UserWorkspaceResponse>> GetWorkspaceMembers(GetWorkspaceUsersQuery request);
        Task<IResult<bool>> AddWorkspaceMember(AddWorkspaceMemberCommand request);
        Task<IResult<bool>> RemoveWorkspaceMember(RemoveWorkspaceMemberCommand request);
        Task<IResult<List<SearchWorkspaceUserResponse>>> SearchWorkspaceMembers(SearchWorkspaceUsersQuery request);
        event EventHandler<WorkspaceDto> OnSelectedWorkspace;
        event EventHandler<WorkspaceAddedEvent> OnWorkspaceAdded;
    }
}
