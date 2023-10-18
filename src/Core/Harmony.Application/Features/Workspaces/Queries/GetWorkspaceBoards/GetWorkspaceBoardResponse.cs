using Harmony.Application.DTO;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Workspaces.Queries.GetWorkspaceBoards
{
    public class GetWorkspaceBoardResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public BoardVisibility Visibility { get; set; }
        public List<BoardListDto> Lists { get; set; }
        public int TotalUsers { get; set; }
        public int TotalItems
        {
            get
            {
                return Lists.SelectMany(l => l.Cards).Sum(c => c.TotalItems);
            }
        }

        public int TotalItemsCompleted
        {
            get
            {
                return Lists.SelectMany(l => l.Cards).Sum(c => c.TotalItemsCompleted);
            }
        }
    }
}
