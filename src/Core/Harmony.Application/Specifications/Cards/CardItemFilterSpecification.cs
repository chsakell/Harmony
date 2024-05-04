using Harmony.Application.Specifications.Base;
using Harmony.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Specifications.Cards
{
    public class CardItemFilterSpecification : HarmonySpecification<Card>
    {
        public Guid? BoardId { get; set; }
        public Guid? CardId { get; set; }
        public List<Guid>? IssueTypes { get; set; }
        public List<Guid>? BoardLists { get; set; }
        public string Title { get; set; }

        public bool IncludeIssueType { get; set; }
        

        public void Build()
        {
            AddCriteria();
            AddInclude();
        }

        private void AddCriteria()
        {
            if (BoardId.HasValue)
            {
                Criteria = And(card => card.IssueType.BoardId == BoardId.Value);
            }

            if (CardId.HasValue)
            {
                Criteria = And(card => card.Id == CardId.Value);
            }

            if (IssueTypes != null && IssueTypes.Any())
            {
                Criteria = And(card => IssueTypes.Contains(card.IssueTypeId.Value));
            }

            if (BoardLists != null && BoardLists.Any())
            {
                Criteria = And(card => BoardLists.Contains(card.BoardListId.Value));
            }

            if (!string.IsNullOrEmpty(Title))
            {
                Criteria = And(card => card.Title.Contains(Title));
            }
        }

        private void AddInclude()
        {
            if (IncludeIssueType)
            {
                Includes.Add(card => card.IssueType);
            }
        }
    }
}
