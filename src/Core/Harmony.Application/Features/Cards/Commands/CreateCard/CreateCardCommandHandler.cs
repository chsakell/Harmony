using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using AutoMapper;
using Harmony.Application.Contracts.Services.Search;
using Harmony.Application.DTO.Search;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications.Email;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Cards.Commands.CreateCard
{
    public class CreateCardCommandHandler : IRequestHandler<CreateCardCommand, Result<CardDto>>
    {
        private readonly ICardRepository _cardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ISearchService _searchService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IStringLocalizer<CreateCardCommandHandler> _localizer;
        private readonly IMapper _mapper;

        public CreateCardCommandHandler(ICardRepository cardRepository,
            ICurrentUserService currentUserService,
            ISearchService searchService,
            INotificationsPublisher notificationsPublisher,
            IStringLocalizer<CreateCardCommandHandler> localizer,
            IMapper mapper)
        {
            _cardRepository = cardRepository;
            _currentUserService = currentUserService;
            _searchService = searchService;
            _notificationsPublisher = notificationsPublisher;
            _localizer = localizer;
            _mapper = mapper;
        }
        public async Task<Result<CardDto>> Handle(CreateCardCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<CardDto>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var totalCards = await _cardRepository.CountCards(request.ListId);
            var nextSerialNumber = await _cardRepository.GetNextSerialNumber(request.BoardId);

            var card = new Card()
            {
                Title = request.Title,
                UserId = userId,
                BoardListId = request.ListId,
                Position = (byte)totalCards,
                SerialNumber = nextSerialNumber,
                IssueTypeId = request.IssueType.Id,
                SprintId = request.SprintId,
            };

            var dbResult = await _cardRepository.CreateAsync(card);

            if (dbResult > 0)
            {
                await _cardRepository.LoadIssueEntryAsync(card);

                _notificationsPublisher
                    .PublishSearchIndexNotification(new CardAddedIndexNotification()
                    {
                        ObjectID = card.Id.ToString(),
                        BoardId = request.BoardId,
                        Title = card.Title,
                        IssueType = request.IssueType.Summary,
                        ListId = request.ListId.ToString(),
                        Status = CardStatus.Active.ToString()
                    });

                var result = _mapper.Map<CardDto>(card);
                return await Result<CardDto>.SuccessAsync(result, _localizer["Card Created"]);
            }

            return await Result<CardDto>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
