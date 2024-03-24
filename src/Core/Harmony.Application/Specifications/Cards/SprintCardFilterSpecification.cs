using Harmony.Domain.Enums;

namespace Harmony.Application.Specifications.Cards
{
    public class SprintCardFilterSpecification : CardFilterSpecification
    {
        public SprintCardFilterSpecification(Guid sprintId, CardIncludes includes = null,
            CardStatus? status = null, string title = null) :
            base(cardId: null, includes ?? new CardIncludes())
        {
            Criteria = card => card.SprintId == sprintId;

            if (status.HasValue)
            {
                And(card => card.Status == status.Value);
            }

            if (!string.IsNullOrEmpty(title))
            {
                And(card => card.Title.ToLower().Contains(title));
            }
        }
    }
}
