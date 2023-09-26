using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;

namespace Harmony.Application.Features.Lists.Commands.UpdateListItemChecked
{
    public class UpdateListItemCheckedCommandHandler : IRequestHandler<UpdateListItemCheckedCommand, Result<bool>>
    {
        private readonly ICheckListItemRepository _checkListItemRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<UpdateListItemCheckedCommandHandler> _localizer;

        public UpdateListItemCheckedCommandHandler(ICheckListItemRepository checkListItemRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<UpdateListItemCheckedCommandHandler> localizer)
        {
            _checkListItemRepository = checkListItemRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }
        public async Task<Result<bool>> Handle(UpdateListItemCheckedCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var listItem = await _checkListItemRepository.Get(request.ListItemId);

            listItem.IsChecked = request.IsChecked;

            var dbResult = await _checkListItemRepository.Update(listItem);

            if (dbResult > 0)
            {
                return await Result<bool>.SuccessAsync(true, _localizer["List item Checked updated"]);
            }

            return await Result<bool>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
