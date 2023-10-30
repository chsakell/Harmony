using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using AutoMapper;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Cards.Commands.CreateChecklist
{
    public class CreateChecklistCommandHandler : IRequestHandler<CreateCheckListCommand, Result<CheckListDto>>
    {
        private readonly ICheckListRepository _checklistRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICardActivityService _cardActivityService;
        private readonly IStringLocalizer<CreateChecklistCommandHandler> _localizer;
        private readonly IMapper _mapper;

        public CreateChecklistCommandHandler(ICheckListRepository checklistRepository,
            ICurrentUserService currentUserService,
            ICardActivityService cardActivityService,
            IStringLocalizer<CreateChecklistCommandHandler> localizer,
            IMapper mapper)
        {
            _checklistRepository = checklistRepository;
            _currentUserService = currentUserService;
            _cardActivityService = cardActivityService;
            _localizer = localizer;
            _mapper = mapper;
        }
        public async Task<Result<CheckListDto>> Handle(CreateCheckListCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<CheckListDto>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var totalCardCheckLists = await _checklistRepository.CountCardCheckLists(request.CardId);

            var checkList = new CheckList
            {
                CardId = request.CardId,
                Title = request.Title,
                Position = (byte)totalCardCheckLists,
                UserId = userId,
            };

            var dbResult = await _checklistRepository.CreateAsync(checkList);

            if (dbResult > 0)
            {
                await _cardActivityService.CreateActivity(checkList.CardId, userId,
                    CardActivityType.CheckListAdded, checkList.DateCreated, checkList.Title);

                var result = _mapper.Map<CheckListDto>(checkList);
                return await Result<CheckListDto>.SuccessAsync(result, _localizer["Card Created"]);
            }

            return await Result<CheckListDto>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
