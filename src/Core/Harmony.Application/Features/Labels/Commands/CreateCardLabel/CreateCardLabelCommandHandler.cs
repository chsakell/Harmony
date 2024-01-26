using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;
using Harmony.Domain.Entities;
using Harmony.Application.Features.Cards.Commands.ToggleCardLabel;

namespace Harmony.Application.Features.Labels.Commands.CreateCardLabel
{
    public class CreateCardLabelCommandHandler : IRequestHandler<CreateCardLabelCommand, IResult<CreateCardLabelResponse>>
    {
        private readonly IBoardLabelRepository _boardLabelRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICardLabelRepository _cardLabelRepository;
        private readonly IStringLocalizer<CreateCardLabelCommandHandler> _localizer;
        private readonly ISender _sender;
        private readonly IMapper _mapper;

        public CreateCardLabelCommandHandler(IBoardLabelRepository boardLabelRepository,
            ICurrentUserService currentUserService,
            ICardLabelRepository cardLabelRepository,
            IStringLocalizer<CreateCardLabelCommandHandler> localizer,
            ISender sender,
            IMapper mapper)
        {
            _boardLabelRepository = boardLabelRepository;
            _currentUserService = currentUserService;
            _cardLabelRepository = cardLabelRepository;
            _localizer = localizer;
            _sender = sender;
            _mapper = mapper;
        }
        public async Task<IResult<CreateCardLabelResponse>> Handle(CreateCardLabelCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<CreateCardLabelResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var label = new Label()
            {
                BoardId = request.BoardId,
                Title = request.Title,
                Colour = request.Color
            };

            var dbResult = await _boardLabelRepository.CreateAsync(label);

            if (dbResult > 0)
            {
                var response = new CreateCardLabelResponse()
                {
                    BoardId = request.BoardId,
                    LabelId = label.Id,
                    Color = request.Color,
                    Title = request.Title
                };

                if(request.CardId.HasValue)
                {
                    var createCardLabelResult = await _sender
                        .Send(new ToggleCardLabelCommand(request.CardId.Value, label.Id)
                        {
                            BoardId = request.BoardId
                        });

                    response.CardId = request.CardId.Value;
                }

                return await Result<CreateCardLabelResponse>.SuccessAsync(response, _localizer["Label created"]);
            }

            return await Result<CreateCardLabelResponse>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
