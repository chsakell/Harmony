using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Cards.Queries.LoadCard
{
    public class LoadCardHandler : IRequestHandler<LoadCardQuery, IResult<LoadCardResponse>>
    {
        private readonly ICardRepository _cardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<LoadCardHandler> _localizer;
        private readonly IMapper _mapper;

        public LoadCardHandler(ICardRepository CardRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<LoadCardHandler> localizer,
            IMapper mapper)
        {
            _cardRepository = CardRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IResult<LoadCardResponse>> Handle(LoadCardQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<LoadCardResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var card = await _cardRepository.Load(request.CardId);

            var result = _mapper.Map<LoadCardResponse>(card);

            return await Result<LoadCardResponse>.SuccessAsync(result);
        }
    }
}
