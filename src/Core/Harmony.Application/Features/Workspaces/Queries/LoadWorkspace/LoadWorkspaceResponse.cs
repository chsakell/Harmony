using Harmony.Application.DTO;

namespace Harmony.Application.Features.Workspaces.Queries.LoadWorkspace
{
    public class LoadWorkspaceResponse
    {
        public List<BoardDto> Boards { get; set; }
        public List<BoardActivityDto> Activities { get; set; }
    }
}
