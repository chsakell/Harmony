using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;

namespace Harmony.Application.Features.Labels.Commands.UpdateTitle
{
    public class UpdateLabelTitleCommandHandler : IRequestHandler<UpdateLabelTitleCommand, IResult>
    {
        private readonly IBoardLabelRepository _boardLabelRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<UpdateLabelTitleCommandHandler> _localizer;
        private readonly IMapper _mapper;

        public UpdateLabelTitleCommandHandler(IBoardLabelRepository boardLabelRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<UpdateLabelTitleCommandHandler> localizer,
            IMapper mapper)
        {
            _boardLabelRepository = boardLabelRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _mapper = mapper;
        }
        public async Task<IResult> Handle(UpdateLabelTitleCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var label = await _boardLabelRepository.GetLabel(request.LabelId);

            if(label == null)
            {
                return await Result.FailAsync(_localizer["Label not found"]);
            }

            label.Title = request.Title;

            var dbResult = await _boardLabelRepository.Update(label);

            if (dbResult > 0)
            {
                return await Result.SuccessAsync(_localizer["Label title updated"]);
            }

            return await Result.FailAsync(_localizer["Operation failed"]);
        }
    }
}
