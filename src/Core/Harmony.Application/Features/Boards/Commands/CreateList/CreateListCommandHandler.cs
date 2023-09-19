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

namespace Harmony.Application.Features.Boards.Commands.Create
{
    public class CreateListCommandHandler : IRequestHandler<CreateListCommand, Result<Guid>>
    {
        private readonly IBoardListRepository _boardListRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<CreateBoardCommandHandler> _localizer;

        public CreateListCommandHandler(IBoardListRepository boardListRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<CreateBoardCommandHandler> localizer)
        {
            _boardListRepository = boardListRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }
        public async Task<Result<Guid>> Handle(CreateListCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<Guid>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var totalLists = await _boardListRepository.CountLists(request.BoardId);

            var boardList = new BoardList()
            {
                Name = request.Name,
                BoardId = request.BoardId,
                Position = (byte)(totalLists + 1)
            };

            var dbResult = await _boardListRepository.Add(boardList);

            if (dbResult > 0)
            {
                return await Result<Guid>.SuccessAsync(boardList.Id, _localizer["List Created"]);
            }

            return await Result<Guid>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
