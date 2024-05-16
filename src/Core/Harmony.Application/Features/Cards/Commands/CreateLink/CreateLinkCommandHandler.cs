using AutoMapper;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using Harmony.Application.DTO.Summaries;
using Harmony.Application.Extensions;
using Harmony.Application.Features.Cards.Commands.RemoveCardAttachment;
using Harmony.Application.Notifications;
using Harmony.Application.Specifications.Cards;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Domain.Extensions;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Harmony.Application.Features.Cards.Commands.CreateLink
{
    public class CreateLinkCommandHandler : IRequestHandler<CreateLinkCommand, Result<LinkDetailsDto>>
    {
        private readonly ILinkRepository _linkRepository;
        private readonly IStringLocalizer<CreateLinkCommandHandler> _localizer;
        private readonly IMapper _mapper;
        private readonly ICardRepository _cardRepository;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly ICacheService _cacheService;
        private readonly ICurrentUserService _currentUserService;

        public CreateLinkCommandHandler(ILinkRepository linkRepository,
            IStringLocalizer<CreateLinkCommandHandler> localizer,
            IMapper mapper,
            ICardRepository cardRepository,
            INotificationsPublisher notificationsPublisher,
            ICacheService cacheService,
            ICurrentUserService currentUserService)
        {
            _linkRepository = linkRepository;
            _localizer = localizer;
            _mapper = mapper;
            _cardRepository = cardRepository;
            _notificationsPublisher = notificationsPublisher;
            _cacheService = cacheService;
            _currentUserService = currentUserService;
        }
        public async Task<Result<LinkDetailsDto>> Handle(CreateLinkCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            var result = new LinkDetailsDto();
            var counterPartResult = new LinkDetailsDto();

            if (string.IsNullOrEmpty(userId) && !_currentUserService.IsTrustedClientRequest)
            {
                return await Result<LinkDetailsDto>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var includes = new CardIncludes() { Board = true };

            var sourceCardFilter = new CardFilterSpecification(request.SourceCardId, includes);

            var sourceCard = await _cardRepository
                .Entities.Specify(sourceCardFilter)
                .FirstOrDefaultAsync();

            if (sourceCard == null)
            {
                return Result<LinkDetailsDto>.Fail("Source issue not found");
            }

            var targetCardFilter = new CardFilterSpecification(request.TargetCardId, includes);

            var targetCard = await _cardRepository
                .Entities.Specify(targetCardFilter)
                .FirstOrDefaultAsync();

            if (targetCard == null)
            {
                return Result<LinkDetailsDto>.Fail("Target issue not found");
            }

            var link = await _linkRepository.GetLink(request.SourceCardId, request.TargetCardId.Value, request.Type);

            if(link != null)
            {
                return Result<LinkDetailsDto>.Fail($"{request.Type.GetDescription()} linking already existing for this card");
            }

            result.SourceCardId = sourceCard.Id;
            result.SourceCardTitle = sourceCard.Title;
            result.SourceCardSerialKey = $"{sourceCard.IssueType.Board.Key}-{sourceCard.SerialNumber}";
            result.SourceCardBoard = _mapper.Map<BoardDto>(sourceCard.IssueType.Board);
            result.TargetCardId = targetCard.Id;
            result.TargetCardTitle = targetCard.Title;
            result.TargetCardSerialKey = $"{targetCard.IssueType.Board.Key}-{targetCard.SerialNumber}";
            result.TargetCardBoard = _mapper.Map<BoardDto>(targetCard.IssueType.Board);
            result.Type = request.Type;

            counterPartResult.SourceCardId = targetCard.Id;
            counterPartResult.SourceCardTitle = targetCard.Title;
            counterPartResult.SourceCardSerialKey = $"{targetCard.IssueType.Board.Key}-{targetCard.SerialNumber}";
            counterPartResult.SourceCardBoard = _mapper.Map<BoardDto>(targetCard.IssueType.Board);
            counterPartResult.TargetCardId = sourceCard.Id;
            counterPartResult.TargetCardTitle = sourceCard.Title;
            counterPartResult.TargetCardSerialKey = $"{sourceCard.IssueType.Board.Key}-{sourceCard.SerialNumber}";
            counterPartResult.TargetCardBoard = _mapper.Map<BoardDto>(sourceCard.IssueType.Board);
            counterPartResult.Type = request.Type.GetCounterPart() ?? request.Type;

            var counterPartLinkType = request.Type.GetCounterPart();
            Link targetLink = null;

            if (counterPartLinkType.HasValue)
            {
                targetLink = new Link()
                {
                    SourceCardId = request.TargetCardId.Value,
                    TargetCardId = request.SourceCardId,
                    UserId = userId,
                    Type = counterPartLinkType.Value,
                };

                await _linkRepository.AddAsync(targetLink);
            }

            var sourceLink = new Link()
            {
                SourceCardId = request.SourceCardId,
                TargetCardId = request.TargetCardId.Value,
                UserId = userId,
                Type = request.Type,
            };

            var dbResult = await _linkRepository.CreateAsync(sourceLink);

            if (dbResult > 0)
            {
                var cardSummaries = await _cacheService.HashMGetFields<Guid, CardSummary>(
                        CacheKeys.ActiveCardSummaries(request.BoardId),
                        new List<string> { request.SourceCardId.ToString(), 
                            request.TargetCardId.ToString() });

                foreach(var cardSummary in cardSummaries.Values)
                {
                    cardSummary.TotalLinks += 1;
                }

                await _cacheService.HashMSetAsync
                    (CacheKeys.ActiveCardSummaries(request.BoardId),
                        cardSummaries);

                result.Id = sourceLink.Id;
                if(targetLink != null)
                {
                    counterPartResult.Id = targetLink.Id;
                }
                
                _notificationsPublisher.PublishMessage(LinkMessageExtensions.GetCreateMessageFromLink(result),
                    NotificationType.CardLinkCreated, 
                    routingKey: BrokerConstants.RoutingKeys.SignalR);

                _notificationsPublisher.PublishMessage(LinkMessageExtensions.GetCreateMessageFromLink(counterPartResult),
                    NotificationType.CardLinkCreated,
                    routingKey: BrokerConstants.RoutingKeys.SignalR);

                return await Result<LinkDetailsDto>.SuccessAsync(result, _localizer["Link created"]);
            }

            return await Result<LinkDetailsDto>.FailAsync(_localizer["Failed to create link"]);
        }
    }
}
