using Harmony.Application.Specifications.Base;
using Harmony.Domain.Entities;

namespace Harmony.Application.Specifications.Cards
{
    public class CardFilterSpecification : HarmonySpecification<Card>
    {
        public CardFilterSpecification(Guid? cardId, CardIncludes includes)
        {
            if(includes.Attachments)
            {
                Includes.Add(card => card.Attachments);
            }

            if (includes.BoardList)
            {
                Includes.Add(card => card.BoardListId);
            }

            if (includes.Board)
            {
                Includes.Add(card => card.BoardList);
                Includes.Add(card => card.IssueType.Board);
            }

            if (includes.Children)
            {
                Includes.Add(card => card.Children);
            }

            if (includes.Members)
            {
                Includes.Add(card => card.Members);
            }

            if (cardId.HasValue)
            {
                Criteria = card => card.Id == cardId;
            }
        }

        public CardFilterSpecification(string term, CardIncludes includes)
        {
            if (includes.Attachments)
            {
                Includes.Add(card => card.Attachments);
            }

            if(includes.IssueType)
            {
                Includes.Add(card => card.IssueType.Board);
            }

            if (includes.Board)
            {
                Includes.Add(card => card.BoardList.Board);
            }
            else if (includes.BoardList)
            {
                Includes.Add(card => card.BoardList);
            }

            if (!string.IsNullOrEmpty(term))
            {
                Criteria = card => card.Title.Contains(term);
            }
        }

        public CardFilterSpecification(CardFilters filters, CardIncludes includes)
        {
            if (includes.Attachments)
            {
                Includes.Add(card => card.Attachments);
            }

            if (includes.Board)
            {
                Includes.Add(card => card.BoardList.Board);
            }
            else if (includes.BoardList)
            {
                Includes.Add(card => card.BoardList);
            }

            if (includes.Members)
            {
                Includes.Add(card => card.Members);
            }

            if (filters.CombineCriteria)
            {
                if (!string.IsNullOrEmpty(filters.Title))
                {
                    Criteria = And(card => card.Title.Contains(filters.Title));
                }

                if (!string.IsNullOrEmpty(filters.Description))
                {
                    Criteria = And(card => card.Description.Contains(filters.Description));
                }

                if (filters.BoardListId.HasValue)
                {
                    Criteria = And(card => card.BoardListId == filters.BoardListId.Value);
                }

                if (filters.HasAttachments)
                {
                    Criteria = And(card => card.Attachments.Count > 0);
                }

                if (filters.BoardId.HasValue && includes.BoardList)
                {
                    Criteria = And(card => card.BoardList.BoardId == filters.BoardId.Value);
                }

                if (filters.BoardId.HasValue && includes.IssueType)
                {
                    Criteria = And(card => card.IssueType.BoardId == filters.BoardId.Value);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(filters.Title))
                {
                    Criteria = Or(card => card.Title.Contains(filters.Title));
                }

                if (!string.IsNullOrEmpty(filters.Description))
                {
                    Criteria = Or(card => card.Description.Contains(filters.Description));
                }

                if (filters.BoardListId.HasValue)
                {
                    Criteria = And(card => card.BoardListId == filters.BoardListId.Value);
                }

                if (filters.BoardId.HasValue && includes.BoardList)
                {
                    Criteria = And(card => card.BoardList.BoardId == filters.BoardId.Value);
                }

                if (filters.BoardId.HasValue && includes.IssueType)
                {
                    Criteria = And(card => card.IssueType.BoardId == filters.BoardId.Value);
                }
            }
        }

        private void SaveOr()
        {

        }

        private void SafeAnd()
        {

        }
    }
}
