using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using AutoMapper;
using Harmony.Application.Features.Cards.Commands.CreateCheckListItem;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Domain.Enums;
using Harmony.Application.Contracts.Services.Hubs;

namespace Harmony.Application.Features.Cards.Commands.CreateChecklist
{
    public class CreateCheckListItemCommandHandler : IRequestHandler<CreateCheckListItemCommand, Result<CheckListItemDto>>
    {
        private readonly ICheckListItemRepository _checkListItemRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICardActivityService _cardActivityService;
        private readonly ICheckListRepository _checklistRepository;
        private readonly IHubClientNotifierService _hubClientNotifierService;
        private readonly ICardRepository _cardRepository;
        private readonly IStringLocalizer<CreateChecklistCommandHandler> _localizer;
        private readonly IMapper _mapper;

        public CreateCheckListItemCommandHandler(ICheckListItemRepository checkListItemRepository,
            ICurrentUserService currentUserService,
            ICardActivityService cardActivityService,
            ICheckListRepository checklistRepository,
            IHubClientNotifierService hubClientNotifierService,
            ICardRepository cardRepository,
            IStringLocalizer<CreateChecklistCommandHandler> localizer,
            IMapper mapper)
        {
            _checkListItemRepository = checkListItemRepository;
            _currentUserService = currentUserService;
            _cardActivityService = cardActivityService;
            _checklistRepository = checklistRepository;
            _hubClientNotifierService = hubClientNotifierService;
            _cardRepository = cardRepository;
            _localizer = localizer;
            _mapper = mapper;
        }
        public async Task<Result<CheckListItemDto>> Handle(CreateCheckListItemCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<CheckListItemDto>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var items = await _checkListItemRepository.GetItems(request.CheckListId);

            var newItem = new CheckListItem()
            {
                CheckListId = request.CheckListId,
                Description = request.Description,
                Position = (byte)items.Count,
                DueDate = request.DueDate,
            };

            var dbResult = await _checkListItemRepository.CreateAsync(newItem);

            if (dbResult > 0)
            {
                var checkList = await _checklistRepository.Get(request.CheckListId);

                await _cardActivityService.CreateActivity(checkList.CardId, userId,
                    CardActivityType.CheckListItemAdded, newItem.DateCreated, checkList.Title);

                var result = _mapper.Map<CheckListItemDto>(newItem);

                var boardId = await _cardRepository.GetBoardId(request.CardId);
                await _hubClientNotifierService
                    .CreateCheckListItem(boardId, request.CardId);

                return await Result<CheckListItemDto>.SuccessAsync(result, _localizer["Item Created"]);
            }

            return await Result<CheckListItemDto>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
