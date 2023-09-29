using Harmony.Application.Features.Workspaces.Commands.Create;
using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
using Harmony.Application.Features.Workspaces.Queries.LoadWorkspace;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IWorkspaceManager : IManager
    {
        Task InitAsync();
        List<GetAllForUserWorkspaceResponse> UserWorkspaces { get; }
        GetAllForUserWorkspaceResponse SelectedWorkspace { get; }
        Task<IResult> CreateAsync(CreateWorkspaceCommand request);
        Task<IResult<List<GetAllForUserWorkspaceResponse>>> GetAllAsync();
        Task<IResult<List<LoadWorkspaceResponse>>> LoadWorkspaceAsync(string workspaceId);
    }
}
