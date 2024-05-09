using Harmony.Application.Specifications.Base;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

namespace Harmony.Application.Specifications.Boards
{
    public class BoardFilterSpecification : HarmonySpecification<Board>
    {
        #region Criteria

        public Guid? BoardId { get; set; }
        public List<BoardListStatus> BoardListsStatuses { get; set; } = new List<BoardListStatus>();
        #endregion

        #region Includes
        public bool IncludeWorkspace { get; set; }
        public bool IncludeLists { get; set; }
        public bool IncludeLabels { get; set; }
        public bool IncludeIssueTypes { get; set; }
        
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
                if (BoardListsStatuses.Any())
                {
                    Includes.Add(Board => Board.Lists
                        .Where(list => BoardListsStatuses.Contains(list.Status)));
                }
                else
                {
                    Includes.Add(Board => Board.Lists);
                }
            }

            if (IncludeIssueTypes)
            {
                Includes.Add(Board => Board.IssueTypes);
            }

            if (IncludeLabels)
            {
                Includes.Add(Board => Board.Labels);
            }
        }
    }
}
