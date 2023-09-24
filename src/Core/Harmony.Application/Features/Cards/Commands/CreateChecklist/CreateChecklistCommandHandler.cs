using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using AutoMapper;

namespace Harmony.Application.Features.Cards.Commands.CreateChecklist
{
    public class CreateChecklistCommandHandler : IRequestHandler<CreateChecklistCommand, Result<CheckListDto>>
    {
        private readonly IChecklistRepository _checklistRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<CreateChecklistCommandHandler> _localizer;
        private readonly IMapper _mapper;

        public CreateChecklistCommandHandler(IChecklistRepository checklistRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<CreateChecklistCommandHandler> localizer,
            IMapper mapper)
        {
            _checklistRepository = checklistRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _mapper = mapper;
        }
        public async Task<Result<CheckListDto>> Handle(CreateChecklistCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<CheckListDto>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var checkList = new CheckList
            {
                CardId = request.CardId,
                Title = request.Title,
                Position = request.Position,
                UserId = userId,
            };

            var dbResult = await _checklistRepository.Add(checkList);

            if (dbResult > 0)
            {
                var result = _mapper.Map<CheckListDto>(checkList);
                return await Result<CheckListDto>.SuccessAsync(result, _localizer["Card Created"]);
            }

            return await Result<CheckListDto>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
