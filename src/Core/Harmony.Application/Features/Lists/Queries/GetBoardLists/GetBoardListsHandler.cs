using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Features.Lists.Queries.GetBoardLists;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Lists.Queries.LoadBoardList
{
    /// <summary>
    /// Handler for returning board lists
    /// </summary>
    public class GetBoardListsHandler : IRequestHandler<GetBoardListsQuery, IResult<List<GetBoardListResponse>>>
    {
        private readonly IBoardListRepository _boardListRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<GetBoardListHandler> _localizer;
        private readonly IMapper _mapper;

        public GetBoardListsHandler(IBoardListRepository boardListRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<GetBoardListHandler> localizer,
            IMapper mapper)
        {
            _boardListRepository = boardListRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IResult<List<GetBoardListResponse>>> Handle(GetBoardListsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {

                return await Result<List<GetBoardListResponse>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var cards = await _boardListRepository.GetBoardLists(request.BoardId);

            var result = _mapper.Map<List<GetBoardListResponse>>(cards);


            return await Result<List<GetBoardListResponse>>.SuccessAsync(result);
        }
    }
}
