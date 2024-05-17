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
using Harmony.Application.Constants;
using Harmony.Application.DTO.Summaries;
using Harmony.Domain.Extensions;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Harmony.Application.Contracts.Services.Caching;

namespace Harmony.Application.Features.Cards.Commands.CreateChecklist
{
    public class CreateChecklistCommandHandler : IRequestHandler<CreateCheckListCommand, Result<CheckListDto>>
    {
        private readonly ICheckListRepository _checklistRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<CreateChecklistCommandHandler> _localizer;
        private readonly ICardSummaryService _cardSummaryService;
        private readonly IMapper _mapper;

        public CreateChecklistCommandHandler(ICheckListRepository checklistRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<CreateChecklistCommandHandler> localizer,
            ICardSummaryService cardSummaryService,
            IMapper mapper)
        {
            _checklistRepository = checklistRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _cardSummaryService = cardSummaryService;
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
                await _cardSummaryService.UpdateCardSummary(request.BoardId, request.CardId,
                (summary) =>
                {
                    summary.CheckLists.Add(new CheckListSummary()
                    {
                        CheckListId = checkList.Id
                    });
                });

                var result = _mapper.Map<CheckListDto>(checkList);
                return await Result<CheckListDto>.SuccessAsync(result, _localizer["Card Created"]);
            }

            return await Result<CheckListDto>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
