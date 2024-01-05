using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using AutoMapper;
using Harmony.Application.Contracts.Services.Search;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Domain.Enums;
using Harmony.Application.Contracts.Services.Management;

namespace Harmony.Application.Features.Cards.Commands.CreateChildIssue
{
    public class CreateChildIssueCommandHandler : IRequestHandler<CreateChildIssueCommand, Result<CardDto>>
    {
        private readonly ICardRepository _cardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ISearchService _searchService;
        private readonly IBoardService _boardService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IStringLocalizer<CreateChildIssueCommandHandler> _localizer;
        private readonly IMapper _mapper;

        public CreateChildIssueCommandHandler(ICardRepository cardRepository,
            ICurrentUserService currentUserService,
            ISearchService searchService,
            IBoardService boardService,
            INotificationsPublisher notificationsPublisher,
            IStringLocalizer<CreateChildIssueCommandHandler> localizer,
            IMapper mapper)
        {
            _cardRepository = cardRepository;
            _currentUserService = currentUserService;
            _searchService = searchService;
            _boardService = boardService;
            _notificationsPublisher = notificationsPublisher;
            _localizer = localizer;
            _mapper = mapper;
        }
        public async Task<Result<CardDto>> Handle(CreateChildIssueCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<CardDto>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var totalCards = await _cardRepository.CountCards(request.ListId.Value);
            var nextSerialNumber = await _cardRepository.GetNextSerialNumber(request.BoardId);

            var card = new Card()
            {
                Title = request.Title,
                UserId = userId,
                BoardListId = request.ListId,
                Position = (byte)totalCards,
                SerialNumber = nextSerialNumber,
                //IssueTypeId = request.IssueType.Id,
            };

            var dbResult = await _cardRepository.CreateAsync(card);

            if (dbResult > 0)
            {
                await _cardRepository.LoadIssueEntryAsync(card);

                var board = await _boardService.GetBoardInfo(request.BoardId);

                _notificationsPublisher
                    .PublishSearchIndexNotification(new CardCreatedIndexNotification()
                    {
                        ObjectID = card.Id.ToString(),
                        BoardId = board.Id,
                        Title = card.Title,
                        IssueType = "Child",
                        ListId = request.ListId.ToString(),
                        Status = CardStatus.Active.ToString(),
                        SerialKey = $"{board.Key}-{card.SerialNumber}"
                    }, board.IndexName);

                var result = _mapper.Map<CardDto>(card);
                return await Result<CardDto>.SuccessAsync(result, _localizer["Child issue created"]);
            }

            return await Result<CardDto>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
