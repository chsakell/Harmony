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
using Harmony.Application.Specifications.Sprints;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Domain.Extensions;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Harmony.Application.Features.Cards.Commands.DeleteLink
{
    public class DeleteLinkCommandHandler : IRequestHandler<DeleteLinkCommand, Result<List<Guid>>>
    {
        private readonly ILinkRepository _linkRepository;
        private readonly IStringLocalizer<DeleteLinkCommandHandler> _localizer;
        private readonly IMapper _mapper;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly ICardRepository _cardRepository;
        private readonly ICacheService _cacheService;
        private readonly ICurrentUserService _currentUserService;

        public DeleteLinkCommandHandler(ILinkRepository linkRepository,
            IStringLocalizer<DeleteLinkCommandHandler> localizer,
            IMapper mapper,
            INotificationsPublisher notificationsPublisher,
            ICardRepository cardRepository,
            ICacheService cacheService,
            ICurrentUserService currentUserService)
        {
            _linkRepository = linkRepository;
            _localizer = localizer;
            _mapper = mapper;
            _notificationsPublisher = notificationsPublisher;
            _cardRepository = cardRepository;
            _cacheService = cacheService;
            _currentUserService = currentUserService;
        }
        public async Task<Result<List<Guid>>> Handle(DeleteLinkCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId) && !_currentUserService.IsTrustedClientRequest)
            {
                return await Result<List<Guid>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            // links to be deleted
            var deletedLinks = new List<Guid>();

            var sourceLink = await _linkRepository.GetLink(request.LinkId);

            if(sourceLink == null)
            {
                return await Result<List<Guid>>.FailAsync("Link not found");
            }

            var targetCardLinkFilter = new LinkFilterSpecification()
            {
                SourceCardId = sourceLink.TargetCardId,
                TargetCardId = sourceLink.SourceCardId,
                Type = sourceLink.Type.GetCounterPart(),
            };

            targetCardLinkFilter.Build();

            var targetCardLink= await _linkRepository
                .Entities.Specify(targetCardLinkFilter)
                .FirstOrDefaultAsync();

            if (targetCardLink != null)
            {
                _linkRepository.Remove(targetCardLink);
                deletedLinks.Add(targetCardLink.Id);
            }
            
            var dbResult = await _linkRepository.Delete(sourceLink);

            if (dbResult > 0)
            {
                var cardSummaries = await _cacheService.HashMGetFields<Guid, CardSummary>(
                        CacheKeys.ActiveCardSummaries(request.BoardId),
                        new List<string> { sourceLink.SourceCardId.ToString(),
                            sourceLink.TargetCardId.ToString() });

                foreach (var cardSummary in cardSummaries.Values)
                {
                    cardSummary.TotalLinks -= 1;
                }

                await _cacheService.HashMSetAsync
                    (CacheKeys.ActiveCardSummaries(request.BoardId),
                        cardSummaries);

                deletedLinks.Add(sourceLink.Id);

                await PublishRemovedLink(sourceLink);
                await PublishRemovedLink(targetCardLink);

                return await Result<List<Guid>>.SuccessAsync(deletedLinks, _localizer["Link removed"]);
            }

            return await Result<List<Guid>>.FailAsync(_localizer["Failed to remove link"]);
        }

        private async Task PublishRemovedLink(Link link)
        {
            if(link == null)
            {
                return;
            }

            var includes = new CardIncludes()
            {
                IssueType = true
            };

            var filter = new CardFilterSpecification(link.SourceCardId, includes);

            var card = await _cardRepository
               .Entities.Specify(filter)
               .FirstOrDefaultAsync();

            if(card == null)
            {
                return;
            }

            _notificationsPublisher.PublishMessage(new CardLinkDeletedMessage(link.Id, card.IssueType.BoardId),
                    NotificationType.CardLinkDeleted,
                    routingKey: BrokerConstants.RoutingKeys.SignalR);
        }
    }
}
