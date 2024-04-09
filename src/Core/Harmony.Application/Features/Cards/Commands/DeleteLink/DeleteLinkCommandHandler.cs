using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using Harmony.Application.Extensions;
using Harmony.Application.Features.Cards.Commands.RemoveCardAttachment;
using Harmony.Application.Specifications.Cards;
using Harmony.Application.Specifications.Sprints;
using Harmony.Domain.Entities;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Harmony.Application.Features.Cards.Commands.DeleteLink
{
    public class DeleteLinkCommandHandler : IRequestHandler<DeleteLinkCommand, Result<List<Guid>>>
    {
        private readonly ILinkRepository _linkRepository;
        private readonly IStringLocalizer<DeleteLinkCommandHandler> _localizer;
        private readonly IMapper _mapper;
        private readonly ICardRepository _cardRepository;
        private readonly ICurrentUserService _currentUserService;

        public DeleteLinkCommandHandler(ILinkRepository linkRepository,
            IStringLocalizer<DeleteLinkCommandHandler> localizer,
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
                deletedLinks.Add(sourceLink.Id);

                return await Result<List<Guid>>.SuccessAsync(deletedLinks, _localizer["Link removed"]);
            }

            return await Result<List<Guid>>.FailAsync(_localizer["Failed to remove link"]);
        }
    }
}
