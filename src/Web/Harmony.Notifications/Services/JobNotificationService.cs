using Harmony.Application.Contracts.Repositories;
using Harmony.Notifications.Contracts;

namespace Harmony.Notifications.Services
{
    public class JobNotificationService : IJobNotificationService
    {
        private readonly IEmailNotificationService _notificationService;
        private readonly ICardRepository _cardRepository;

        public JobNotificationService(IEmailNotificationService notificationService,
            ICardRepository cardRepository)
        {
            _notificationService = notificationService;
            _cardRepository = cardRepository;
        }

        public async Task SendCardDueDateChangedNotification(Guid cardId)
        {
            var card = await _cardRepository.Get(cardId);
        }
    }
}
