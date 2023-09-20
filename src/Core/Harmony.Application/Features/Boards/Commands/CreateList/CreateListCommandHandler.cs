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

namespace Harmony.Application.Features.Boards.Commands.Create
{
    public class CreateListCommandHandler : IRequestHandler<CreateListCommand, Result<BoardListDto>>
    {
        private readonly IBoardListRepository _boardListRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<CreateBoardCommandHandler> _localizer;
		private readonly IMapper _mapper;

		public CreateListCommandHandler(IBoardListRepository boardListRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<CreateBoardCommandHandler> localizer,
            IMapper mapper)
        {
            _boardListRepository = boardListRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
			_mapper = mapper;
		}
        public async Task<Result<BoardListDto>> Handle(CreateListCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<BoardListDto>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var totalLists = await _boardListRepository.CountLists(request.BoardId);

            var boardList = new BoardList()
            {
                Name = request.Name,
                UserId = userId,
                BoardId = request.BoardId,
                Position = (byte)(totalLists + 1)
            };

            var dbResult = await _boardListRepository.Add(boardList);

            if (dbResult > 0)
            {
                var result = _mapper.Map<BoardListDto>(boardList);
                return await Result<BoardListDto>.SuccessAsync(result, _localizer["List Created"]);
            }

            return await Result<BoardListDto>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
