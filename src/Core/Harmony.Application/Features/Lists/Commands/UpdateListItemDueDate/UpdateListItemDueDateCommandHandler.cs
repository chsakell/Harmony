using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;

namespace Harmony.Application.Features.Lists.Commands.UpdateListItemDueDate
{
    public class UpdateListItemDueDateCommandHandler : IRequestHandler<UpdateListItemDueDateCommand, Result<bool>>
    {
        private readonly ICheckListItemRepository _checkListItemRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<UpdateListItemDueDateCommandHandler> _localizer;

        public UpdateListItemDueDateCommandHandler(ICheckListItemRepository checkListItemRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<UpdateListItemDueDateCommandHandler> localizer)
        {
            _checkListItemRepository = checkListItemRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }
        public async Task<Result<bool>> Handle(UpdateListItemDueDateCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var listItem = await _checkListItemRepository.Get(request.ListItemId);

            listItem.DueDate = request.DueDate;

            var dbResult = await _checkListItemRepository.Update(listItem);

            if (dbResult > 0)
            {
                return await Result<bool>.SuccessAsync(true, _localizer["List item DueDate updated"]);
            }

            return await Result<bool>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
