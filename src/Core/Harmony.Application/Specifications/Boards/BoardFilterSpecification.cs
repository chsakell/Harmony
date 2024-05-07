using Harmony.Application.Specifications.Base;
using Harmony.Domain.Entities;

namespace Harmony.Application.Specifications.Boards
{
    public class BoardFilterSpecification : HarmonySpecification<Board>
    {
        #region Criteria

        public Guid? BoardId { get; set; }

        #endregion

        #region
        public bool IncludeWorkspace { get; set; }
        public bool IncludeLists { get; set; }
        #endregion

        public void Build()
        {
            AddCriteria();
            AddIncludes();
        }

        private void AddCriteria()
        {
            if (BoardId.HasValue)
            {
                Criteria = Board => Board.Id == BoardId;
            }
        }

        private void AddIncludes()
        {
            if (IncludeWorkspace)
            {
                Includes.Add(Board => Board.Workspace);
            }

            if (IncludeLists)
            {
                Includes.Add(Board => Board.Lists);
            }
        }
    }
}
