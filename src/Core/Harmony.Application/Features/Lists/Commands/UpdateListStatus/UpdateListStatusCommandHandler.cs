using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;
using Harmony.Application.Contracts.Services.Management;

namespace Harmony.Application.Features.Lists.Commands.ArchiveList
{
    public class ArchiveListCommandHandler : IRequestHandler<UpdateListStatusCommand, Result<bool>>
    {
        private readonly IBoardListRepository _boardListRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<ArchiveListCommandHandler> _localizer;
        private readonly IListService _listService;
        private readonly IMapper _mapper;

        public ArchiveListCommandHandler(IBoardListRepository boardListRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<ArchiveListCommandHandler> localizer,
            IListService listService,
            IMapper mapper)
        {
            _boardListRepository = boardListRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _listService = listService;
            _mapper = mapper;
        }
        public async Task<Result<bool>> Handle(UpdateListStatusCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var list = await _boardListRepository.Get(request.ListId);

            list.Status = request.Status;

            if(request.Status == Domain.Enums.BoardListStatus.Archived)
            {
                await _listService.ReorderAfterArchive(list);
            }

            var dbResult = await _boardListRepository.Update(list);

            if (dbResult > 0)
            {
                return await Result<bool>.SuccessAsync(true, _localizer["List status updated"]);
            }

            return await Result<bool>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
