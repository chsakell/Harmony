using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;

namespace Harmony.Application.Features.Lists.Commands.UpdateListTitle
{
    public class UpdateListTitleCommandHandler : IRequestHandler<UpdateListTitleCommand, Result<bool>>
    {
        private readonly ICheckListRepository _checkListRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<UpdateListTitleCommandHandler> _localizer;
        private readonly IMapper _mapper;

        public UpdateListTitleCommandHandler(ICheckListRepository checkListRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<UpdateListTitleCommandHandler> localizer,
            IMapper mapper)
        {
            _checkListRepository = checkListRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _mapper = mapper;
        }
        public async Task<Result<bool>> Handle(UpdateListTitleCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var list = await _checkListRepository.Get(request.ListId);

            list.Title = request.Title;

            var dbResult = await _checkListRepository.Update(list);

            if (dbResult > 0)
            {
                return await Result<bool>.SuccessAsync(true, _localizer["List title updated"]);
            }

            return await Result<bool>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
