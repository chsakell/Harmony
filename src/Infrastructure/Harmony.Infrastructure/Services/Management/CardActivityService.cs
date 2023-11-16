using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Services.Management
{
    public class CardActivityService : ICardActivityService
    {
        private readonly UserManager<HarmonyUser> _userManager;
        private readonly ICardRepository _cardRepository;
        private readonly IBoardListRepository _boardListRepository;
        private readonly ICardActivityRepository _cardActivityRepository;

        public CardActivityService(UserManager<HarmonyUser> userManager,
            ICardRepository cardRepository,
            IBoardListRepository boardListRepository,
            ICardActivityRepository cardActivityRepository)
        {
            _userManager = userManager;
            _cardRepository = cardRepository;
            _boardListRepository = boardListRepository;
            _cardActivityRepository = cardActivityRepository;
        }

        public async Task<List<CardActivityDto>> GetAsync(Guid cardId)
        {
            var query = from cardActivity in _cardActivityRepository.Entities
                        join user in _userManager.Users on cardActivity.UserId equals user.Id
                        where cardActivity.CardId == cardId
                        select new CardActivityDto
                        {
                            Id = cardActivity.Id,
                            CardId = cardActivity.CardId,
                            Activity = cardActivity.Activity,
                            Actor = $"{user.FirstName} {user.LastName}",
                            DateCreated = cardActivity.DateCreated,
                            Type = cardActivity.Type,
                            Url = cardActivity.Url,
                        };

            return await query.ToListAsync();
        }

        public async Task CreateActivity(Guid cardId, string userId, CardActivityType type,
            DateTime date, string? extraInfo = null, string url = null)
        {
            var activity = new CardActivity()
            {
                CardId = cardId,
                Type = type,
                UserId = userId,
                Url = url,
            };

            switch (type)
            {
                case CardActivityType.CardTitleUpdated:
                    activity.Activity = $"Updated card's title to <b>{extraInfo}</b>";
                    break;
                case CardActivityType.CardDescriptionUpdated:
                    activity.Activity = $"Updated card's description";
                    break;
                case CardActivityType.CardDatesUpdated:
                    activity.Activity = $"Updated card's dates to <b>{extraInfo}</b>";
                    break;
                case CardActivityType.CheckListAdded:
                    activity.Activity = $"Created check list <b>{extraInfo}</b>";
                    break;
                case CardActivityType.CheckListItemAdded:
                    activity.Activity = $"Added an item in <b>{extraInfo}</b>";
                    break;
                case CardActivityType.ImageAttachmentAdded:
                case CardActivityType.DocumentAttachmentAdded:
                    activity.Activity = $"Attached <b>{extraInfo}</b>";
                    break;
                default:
                    activity = null;
                    break;
            }

            if (activity == null)
            {
                return;
            }

            await _cardActivityRepository.CreateAsync(activity);
        }

        public async Task<List<BoardActivityDto>> GetBoardsActivities(List<Guid> boardIds)
        {
            var query = from cardActivity in _cardActivityRepository.Entities
                        join card in _cardRepository.Entities on cardActivity.CardId equals card.Id
                        join boardList in _boardListRepository.Entities on card.BoardListId equals boardList.Id
                        join user in _userManager.Users on cardActivity.UserId equals user.Id
                        where boardIds.Contains(boardList.BoardId)
                        orderby boardList.BoardId, cardActivity.DateCreated
                        select new BoardActivityDto
                        {
                            Id = cardActivity.Id,
                            BoardId = boardList.BoardId,
                            CardId = cardActivity.CardId,
                            Activity = cardActivity.Activity,
                            Actor = $"{user.FirstName} {user.LastName}",
                            DateCreated = cardActivity.DateCreated,
                            Type = cardActivity.Type,
                            Url = cardActivity.Url,
                        };

            return await query.ToListAsync();
        }
    }
}
