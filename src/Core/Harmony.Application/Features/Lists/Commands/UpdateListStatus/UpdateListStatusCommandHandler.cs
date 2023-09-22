using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Features.Boards.Commands.CreateList;
using Harmony.Application.DTO;
using AutoMapper;
using Harmony.Application.Features.Boards.Commands.Create;

namespace Harmony.Application.Features.Lists.Commands.ArchiveList
{
    public class ArchiveListCommandHandler : IRequestHandler<UpdateListStatusCommand, Result<bool>>
    {
        private readonly IBoardListRepository _boardListRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<ArchiveListCommandHandler> _localizer;
        private readonly IMapper _mapper;

        public ArchiveListCommandHandler(IBoardListRepository boardListRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<ArchiveListCommandHandler> localizer,
            IMapper mapper)
        {
            _boardListRepository = boardListRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
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

            var dbResult = await _boardListRepository.Update(list);

            if (dbResult > 0)
            {
                return await Result<bool>.SuccessAsync(true, _localizer["List status updated"]);
            }

            return await Result<bool>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
