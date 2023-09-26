using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;

namespace Harmony.Application.Features.Lists.Commands.UpdateListItemDescription
{
    public class UpdateListItemDescriptionCommandHandler : IRequestHandler<UpdateListItemDescriptionCommand, Result<bool>>
    {
        private readonly ICheckListItemRepository _checkListItemRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<UpdateListItemDescriptionCommandHandler> _localizer;
        private readonly IMapper _mapper;

        public UpdateListItemDescriptionCommandHandler(ICheckListItemRepository checkListItemRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<UpdateListItemDescriptionCommandHandler> localizer,
            IMapper mapper)
        {
            _checkListItemRepository = checkListItemRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _mapper = mapper;
        }
        public async Task<Result<bool>> Handle(UpdateListItemDescriptionCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var listItem = await _checkListItemRepository.Get(request.ListItemId);

            listItem.Description = request.Description;

            var dbResult = await _checkListItemRepository.Update(listItem);

            if (dbResult > 0)
            {
                return await Result<bool>.SuccessAsync(true, _localizer["List item description updated"]);
            }

            return await Result<bool>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
