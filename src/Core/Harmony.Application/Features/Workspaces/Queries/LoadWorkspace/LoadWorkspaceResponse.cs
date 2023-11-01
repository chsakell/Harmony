using Harmony.Application.DTO;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Workspaces.Queries.LoadWorkspace
{
    public class LoadWorkspaceResponse
    {
        public List<BoardDto> Boards { get; set; }
    }
}
