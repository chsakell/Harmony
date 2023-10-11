using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Infrastructure.Services.Management
{
    public class CardActivityService : ICardActivityService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ICardActivityRepository _cardActivityRepository;

        public CardActivityService(ICurrentUserService currentUserService,
            ICardActivityRepository cardActivityRepository)
        {
            _currentUserService = currentUserService;
            _cardActivityRepository = cardActivityRepository;
        }

        public async Task CreateActivity(Guid cardId, string userId, CardActivityType type, DateTime date)
        {
            var activity = new CardActivity()
            {
                CardId = cardId,
                Type = type,
                UserId = userId
            };

            switch (type)
            {
                case CardActivityType.CardTitleUpdated:
                    activity.Activity = $"{_currentUserService.FullName} updated card's title at {date.ToString("ddd, dd MMM yyyy HH:mm")}";
                    break;

                default:
                    activity = null;
                    break;
            }

            if(activity == null)
            {
                return;
            }

            await _cardActivityRepository.CreateAsync(activity);
        }
    }
}
