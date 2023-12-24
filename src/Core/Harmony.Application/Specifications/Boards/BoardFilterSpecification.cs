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

            if (includes.Lists)
            {
                Includes.Add(Board => Board.Lists);
            }

            if (boardId.HasValue)
            {
                Criteria = Board => Board.Id == boardId;
            }
        }

        public BoardFilterSpecification(List<Guid> boardIds, BoardIncludes includes)
        {
            if (includes.Workspace)
            {
                Includes.Add(Board => Board.Workspace);
            }

            if (includes.Lists)
            {
                Includes.Add(Board => Board.Lists);
            }

            if (boardIds != null)
            {
                Criteria = Board => boardIds.Contains(Board.Id);
            }
        }
    }
}
