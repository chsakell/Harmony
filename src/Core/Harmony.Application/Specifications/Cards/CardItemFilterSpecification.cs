using Harmony.Application.Specifications.Base;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
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
        public List<Guid>? Sprints { get; set; }
        public string Title { get; set; }
        public List<CardStatus>? Statuses { get; set; }
        public bool SkipChildren { get; set; }

        public bool IncludeIssueType { get; set; }
        public bool IncludeLabels { get; set; }
        public bool IncludeCheckLists { get; set; }
        public bool IncludeAttachments { get; set; }
        public bool IncludeLinks { get; set; }

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

            if (Statuses != null && Statuses.Any())
            {
                Criteria = And(card => Statuses.Contains(card.Status));
            }

            if (BoardLists != null && BoardLists.Any())
            {
                Criteria = And(card => BoardLists.Contains(card.BoardListId.Value));
            }

            if (Sprints != null && Sprints.Any())
            {
                Criteria = And(card => Sprints.Contains(card.SprintId.Value));
            }

            if (!string.IsNullOrEmpty(Title))
            {
                Criteria = And(card => card.Title.Contains(Title));
            }

            if (SkipChildren)
            {
                Criteria = And(card => card.ParentCardId == null);
            }
        }

        private void AddInclude()
        {
            if (IncludeIssueType)
            {
                Includes.Add(card => card.IssueType);
            }

            if (IncludeLabels)
            {
                Includes.Add(card => card.Labels);
            }

            if (IncludeCheckLists)
            {
                Includes.Add(card => card.CheckLists);
            }

            if (IncludeAttachments)
            {
                Includes.Add(card => card.Attachments);
            }

            if (IncludeLinks)
            {
                Includes.Add(card => card.Links);
            }
        }
    }
}
