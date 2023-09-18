using Harmony.Application.Features.Workspaces.Commands.Create;
using Harmony.Application.Features.Workspaces.Queries.GetAll;
using Harmony.Application.Requests.Identity;
using Harmony.Shared.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IWorkspaceManager : IManager
    {
        Task<IResult> CreateAsync(CreateWorkspaceCommand request);
        Task<IResult<List<GetUserOwnedWorkspacesResponse>>> GetAllAsync();
    }
}
