using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Cards.Queries.GetLabels
{
    public class GetCardLabelsHandler : IRequestHandler<GetCardLabelsQuery, IResult<GetCardLabelsResponse>>
    {
        private readonly ICardRepository _cardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBoardLabelRepository _boardLabelRepository;
        private readonly ICardLabelRepository _cardLabelRepository;
        private readonly IStringLocalizer<GetCardLabelsHandler> _localizer;
        private readonly IMapper _mapper;

        public GetCardLabelsHandler(ICardRepository cardRepository,
            ICurrentUserService currentUserService,
            IBoardLabelRepository boardLabelRepository,
            ICardLabelRepository cardLabelRepository,
            IStringLocalizer<GetCardLabelsHandler> localizer,
            IMapper mapper)
        {
            _cardRepository = cardRepository;
            _currentUserService = currentUserService;
            _boardLabelRepository = boardLabelRepository;
            _cardLabelRepository = cardLabelRepository;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IResult<GetCardLabelsResponse>> Handle(GetCardLabelsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<GetCardLabelsResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var boardId = await _cardRepository.GetBoardId(request.CardId);

            var boardLabels = await _boardLabelRepository.GetLabels(boardId);

            var cardLabels = await _cardLabelRepository.GetLabels(request.CardId);

            var result = new GetCardLabelsResponse();

            foreach (var label in boardLabels)
            {
                result.BoardLabels.Add(_mapper.Map<LabelDto>(label));
            }

            foreach(var cardLabel in cardLabels)
            {
                result.CardLabels.Add(cardLabel.Label.Id);
            }

            return await Result<GetCardLabelsResponse>.SuccessAsync(result);
        }
    }
}
