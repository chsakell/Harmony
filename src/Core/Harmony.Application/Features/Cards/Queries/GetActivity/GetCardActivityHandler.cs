using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Cards.Queries.GetActivity
{
    public class GetCardActivityHandler : IRequestHandler<GetCardActivityQuery, IResult<List<CardActivityDto>>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<GetCardActivityHandler> _localizer;
        private readonly ICardActivityRepository _cardActivityRepository;
        private readonly ICardActivityService _cardActivityService;
        private readonly IMapper _mapper;

        public GetCardActivityHandler(ICurrentUserService currentUserService,
            IStringLocalizer<GetCardActivityHandler> localizer,
            ICardActivityRepository cardActivityRepository,
            ICardActivityService cardActivityService,
            IMapper mapper)
        {

            _currentUserService = currentUserService;
            _localizer = localizer;
            _cardActivityRepository = cardActivityRepository;
            _cardActivityService = cardActivityService;
            _mapper = mapper;
        }

        public async Task<IResult<List<CardActivityDto>>> Handle(GetCardActivityQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<List<CardActivityDto>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var result = await _cardActivityService.GetAsync(request.CardId);

            return await Result<List<CardActivityDto>>.SuccessAsync(result);
        }
    }
}
