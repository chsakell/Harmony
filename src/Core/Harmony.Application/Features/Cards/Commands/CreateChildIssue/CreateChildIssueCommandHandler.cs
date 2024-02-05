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
using Harmony.Application.Extensions;
using Harmony.Application.Specifications.Cards;
using Microsoft.EntityFrameworkCore;
using Harmony.Shared.Constants.Application;
using Harmony.Application.Features.Workspaces.Queries.GetIssueTypes;
using Harmony.Application.Constants;
using Harmony.Application.Notifications;

namespace Harmony.Application.Features.Cards.Commands.CreateChildIssue
{
    public class CreateChildIssueCommandHandler : IRequestHandler<CreateChildIssueCommand, Result<CardDto>>
    {
        private readonly ICardRepository _cardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ISearchService _searchService;
        private readonly IBoardService _boardService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IIssueTypeRepository _issueTypeRepository;
        private readonly IStringLocalizer<CreateChildIssueCommandHandler> _localizer;
        private readonly ISender _sender;
        private readonly IMapper _mapper;

        public CreateChildIssueCommandHandler(ICardRepository cardRepository,
            ICurrentUserService currentUserService,
            ISearchService searchService,
            IBoardService boardService,
            INotificationsPublisher notificationsPublisher,
            IIssueTypeRepository issueTypeRepository,
            IStringLocalizer<CreateChildIssueCommandHandler> localizer,
            ISender sender, IMapper mapper)
        {
            _cardRepository = cardRepository;
            _currentUserService = currentUserService;
            _searchService = searchService;
            _boardService = boardService;
            _notificationsPublisher = notificationsPublisher;
            _issueTypeRepository = issueTypeRepository;
            _localizer = localizer;
            _sender = sender;
            _mapper = mapper;
        }
        public async Task<Result<CardDto>> Handle(CreateChildIssueCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<CardDto>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var includes = new CardIncludes() { Children = true };

            var filter = new CardFilterSpecification(request.CardId, includes);

            var card = await _cardRepository
                .Entities.Specify(filter)
                .FirstOrDefaultAsync();

            if (card == null)
            {
                return Result<CardDto>.Fail("Card not found");
            }

            var totalChildren = card.Children.Count;

            var boardIssueTypes = (await _sender.Send(new GetIssueTypesQuery(request.BoardId) { NormalOnly = false })).Data;

            var subTaskIssueType = boardIssueTypes
                .FirstOrDefault(type => type.Summary.Equals(IssueTypesConstants.SUBTASK));

            var nextSerialNumber = await _cardRepository.GetNextSerialNumber(request.BoardId);

            var childIssue = new Card()
            {
                Title = request.Title,
                UserId = userId,
                BoardListId = request.ListId,
                Position = (byte)totalChildren,
                SerialNumber = nextSerialNumber,
                ParentCardId = request.CardId,
                IssueTypeId = subTaskIssueType?.Id
            };

            var dbResult = await _cardRepository.CreateAsync(childIssue);

            if (dbResult > 0)
            {
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

                var result = _mapper.Map<CardDto>(childIssue);

                var cardCreatedNotification = new CardCreatedMessage(board.Id, result, userId);

                _notificationsPublisher.PublishMessage(cardCreatedNotification,
                    NotificationType.CardCreated, routingKey: BrokerConstants.RoutingKeys.Notifications);

                return await Result<CardDto>.SuccessAsync(result, _localizer["Child issue created"]);
            }

            return await Result<CardDto>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
