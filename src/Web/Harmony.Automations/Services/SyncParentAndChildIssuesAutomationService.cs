using Harmony.Application.Contracts.Automation;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.DTO.Automation;
using Harmony.Application.Notifications;
using Harmony.Application.Specifications.Cards;
using Harmony.Application.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Automations.Services
{
    public class SyncParentAndChildIssuesAutomationService : ISyncParentAndChildIssuesAutomationService
    {
        private readonly ICardRepository _cardRepository;

        public SyncParentAndChildIssuesAutomationService(ICardRepository cardRepository)
        {
            _cardRepository = cardRepository;
        }

        public async Task Process(SyncParentAndChildIssuesAutomationDto automation, CardMovedMessage notification)
        {
            if(!notification.ParentCardId.HasValue)
            {
                return;
            }

            var includes = new CardIncludes() { Children = true };

            var filter = new CardFilterSpecification(notification.ParentCardId, includes);

            var card = await _cardRepository
                .Entities.Specify(filter)
                .FirstOrDefaultAsync();

            if(notification.MovedToListId != card.BoardListId)
            {
                // check all children have the same status
                var allChildrenHaveSameStatus = card.Children.All(c => c.BoardListId == notification.MovedToListId); ;

                if(allChildrenHaveSameStatus && automation.ToStatuses
                    .Contains(notification.MovedFromListId.ToString()))
                {
                    card.BoardListId = notification.MovedToListId;

                    await _cardRepository.Update(card);
                }
            }

            return;
        }
    }
}
