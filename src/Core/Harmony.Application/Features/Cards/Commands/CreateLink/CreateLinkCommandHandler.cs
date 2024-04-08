using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using Harmony.Application.Extensions;
using Harmony.Application.Features.Cards.Commands.RemoveCardAttachment;
using Harmony.Application.Specifications.Cards;
using Harmony.Domain.Entities;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Harmony.Application.Features.Cards.Commands.CreateLink
{
    public class CreateLinkCommandHandler : IRequestHandler<CreateLinkCommand, Result<LinkDto>>
    {
        private readonly ILinkRepository _linkRepository;
        private readonly IStringLocalizer<CreateLinkCommandHandler> _localizer;
        private readonly IMapper _mapper;
        private readonly ICardRepository _cardRepository;
        private readonly ICurrentUserService _currentUserService;

        public CreateLinkCommandHandler(ILinkRepository linkRepository,
            IStringLocalizer<CreateLinkCommandHandler> localizer,
            IMapper mapper,
            ICardRepository cardRepository,
            ICurrentUserService currentUserService)
        {
            _linkRepository = linkRepository;
            _localizer = localizer;
            _mapper = mapper;
            _cardRepository = cardRepository;
            _currentUserService = currentUserService;
        }
        public async Task<Result<LinkDto>> Handle(CreateLinkCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            var result = new LinkDto();

            if (string.IsNullOrEmpty(userId) && !_currentUserService.IsTrustedClientRequest)
            {
                return await Result<LinkDto>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var includes = new CardIncludes() { Board = true };

            var sourceCardFilter = new CardFilterSpecification(request.SourceCardId, includes);

            var sourceCard = await _cardRepository
                .Entities.Specify(sourceCardFilter)
                .FirstOrDefaultAsync();

            if (sourceCard == null)
            {
                return Result<LinkDto>.Fail("Source issue not found");
            }

            var targetCardFilter = new CardFilterSpecification(request.TargetCardId, includes);

            var targetCard = await _cardRepository
                .Entities.Specify(targetCardFilter)
                .FirstOrDefaultAsync();

            if (targetCard == null)
            {
                return Result<LinkDto>.Fail("Target issue not found");
            }

            var link = await _linkRepository.GetLink(request.SourceCardId, request.TargetCardId.Value, request.Type);

            if(link != null)
            {
                return Result<LinkDto>.Fail($"{request.Type.GetDescription()} linking already existing for this card");
            }

            result.SourceCardId = sourceCard.Id;
            result.SourceCardTitle = sourceCard.Title;
            result.SourceCardSerialKey = $"{sourceCard.IssueType.Board.Key}-{sourceCard.SerialNumber}";
            result.SourceCardBoard = _mapper.Map<BoardDto>(sourceCard.IssueType.Board);
            result.TargetCardId = targetCard.Id;
            result.TargetCardTitle = targetCard.Title;
            result.TargetCardSerialKey = $"{targetCard.IssueType.Board.Key}-{targetCard.SerialNumber}";
            result.TargetCardBoard = _mapper.Map<BoardDto>(targetCard.IssueType.Board);
            result.Type = result.Type;

            var counterPartLinkType = request.Type.GetCounterPart();

            if (counterPartLinkType.HasValue)
            {
                var targetLink = new Link()
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

                return await Result<LinkDto>.SuccessAsync(result, _localizer["Link created"]);
            }

            return await Result<LinkDto>.FailAsync(_localizer["Failed to create link"]);
        }
    }
}
