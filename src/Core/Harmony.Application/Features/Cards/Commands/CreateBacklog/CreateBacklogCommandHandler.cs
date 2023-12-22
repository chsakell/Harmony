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
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Cards.Commands.CreateBacklog
{
    public class CreateBacklogCommandHandler : IRequestHandler<CreateBacklogCommand, Result<CardDto>>
    {
        private readonly ICardRepository _cardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ISearchService _searchService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IStringLocalizer<CreateBacklogCommandHandler> _localizer;
        private readonly IMapper _mapper;

        public CreateBacklogCommandHandler(ICardRepository cardRepository,
            ICurrentUserService currentUserService,
            ISearchService searchService,
            INotificationsPublisher notificationsPublisher,
            IStringLocalizer<CreateBacklogCommandHandler> localizer,
            IMapper mapper)
        {
            _cardRepository = cardRepository;
            _currentUserService = currentUserService;
            _searchService = searchService;
            _notificationsPublisher = notificationsPublisher;
            _localizer = localizer;
            _mapper = mapper;
        }
        public async Task<Result<CardDto>> Handle(CreateBacklogCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<CardDto>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var totalBacklogCards = await _cardRepository.CountBacklogCards(request.BoardId);

            var nextSerialNumber = (await _cardRepository.CountBoardCards(request.BoardId)) + 1;

            var card = new Card()
            {
                Title = request.Title,
                UserId = userId,
                Position = (byte)totalBacklogCards,
                SerialNumber = nextSerialNumber,
                Status = Domain.Enums.CardStatus.Backlog,
                IssueTypeId = request.IssueType.Id,
            };

            var dbResult = await _cardRepository.CreateAsync(card);
            if (dbResult > 0)
            {
                _notificationsPublisher
                    .PublishSearchIndexNotification(new CardAddedIndexNotification()
                    {
                        ObjectID = card.Id.ToString(),
                        BoardId = request.BoardId,
                        Title = card.Title,
                        IssueType = request.IssueType.Summary,
                        Status = CardStatus.Active.ToString()
                    });

                var result = _mapper.Map<CardDto>(card);
                return await Result<CardDto>.SuccessAsync(result, _localizer["Issue Created"]);
            }

            return await Result<CardDto>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
