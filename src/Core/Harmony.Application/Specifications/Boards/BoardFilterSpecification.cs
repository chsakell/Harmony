using Harmony.Application.Specifications.Base;
using Harmony.Domain.Entities;

namespace Harmony.Application.Specifications.Boards
{
    public class BoardFilterSpecification : HarmonySpecification<Board>
    {
        public BoardFilterSpecification(Guid? boardId, BoardIncludes includes)
        {
            if(includes.Workspace)
            {
                Includes.Add(Board => Board.Workspace);
            }

            if(boardId.HasValue)
            {
                Criteria = Board => Board.Id == boardId;
            }
        }
    }
}
