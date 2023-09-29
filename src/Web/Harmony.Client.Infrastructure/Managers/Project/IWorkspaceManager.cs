using Harmony.Application.DTO;
using Harmony.Application.Features.Workspaces.Commands.Create;
using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
using Harmony.Application.Features.Workspaces.Queries.LoadWorkspace;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IWorkspaceManager : IManager
    {
        Task InitAsync();
        List<WorkspaceDto> UserWorkspaces { get; }
        WorkspaceDto SelectedWorkspace { get; }
        Task<IResult<Guid>> CreateAsync(CreateWorkspaceCommand request);
        Task SelectWorkspace(Guid id);
        Task<IResult<List<WorkspaceDto>>> GetAllAsync();
        Task<IResult<List<LoadWorkspaceResponse>>> LoadWorkspaceAsync(string workspaceId);
        event EventHandler<WorkspaceDto> OnSelectedWorkspace;
    }
}
