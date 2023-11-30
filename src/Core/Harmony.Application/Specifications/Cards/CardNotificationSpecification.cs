using Harmony.Application.Specifications.Base;
using Harmony.Domain.Entities;

namespace Harmony.Application.Specifications.Cards
{
    public class CardNotificationSpecification : HarmonySpecification<Card>
    {
        public CardNotificationSpecification(Guid cardId)
        {
            Includes.Add(card => card.Members);
            Includes.Add(card => card.BoardList);
            Includes.Add(card => card.BoardList.Board);

            Criteria = card => card.Id == cardId;
        }
    }
}
