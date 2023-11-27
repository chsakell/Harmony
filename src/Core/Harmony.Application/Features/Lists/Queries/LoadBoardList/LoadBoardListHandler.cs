using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Lists.Queries.LoadBoardList
{
    /// <summary>
    /// Handler for returning board list cards
    /// </summary>
    public class GetBoardListHandler : IRequestHandler<LoadBoardListQuery, IResult<List<CardDto>>>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IBoardListRepository _boardListRepository;
        private readonly IBoardService _boardService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<GetBoardListHandler> _localizer;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetBoardListHandler(IBoardRepository boardRepository,
            IBoardListRepository boardListRepository,
            IBoardService boardService,
            ICurrentUserService currentUserService,
            IStringLocalizer<GetBoardListHandler> localizer,
            IUserService userService,
            IMapper mapper)
        {
            _boardRepository = boardRepository;
            _boardListRepository = boardListRepository;
            _boardService = boardService;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<IResult<List<CardDto>>> Handle(LoadBoardListQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {

                return await Result<List<CardDto>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var boardExists = await _boardRepository.Exists(request.BoardId);

            if (!boardExists)
            {
                return await Result<List<CardDto>>.FailAsync(_localizer["Board doesn't exist"]);
            }

            var cards = await _boardService
                    .LoadBoardListCards(request.BoardId, request.BoardListId, request.Page, request.PageSize);

            var result = _mapper.Map<List<CardDto>>(cards);

            var cardUserIds = result.SelectMany(c => c.Members)
                    .Select(m => m.Id).Distinct();

            var cardUsers = (await _userService.GetAllAsync(cardUserIds)).Data;

            foreach (var card in result.Where(c => c.Members.Any()))
            {
                var users = cardUsers.Where(u => card.Members.Select(m => m.Id).Contains(u.Id)).Distinct();
                card.Members = _mapper.Map<List<CardMemberDto>>(users);
            }

            return await Result<List<CardDto>>.SuccessAsync(result);
        }
    }
}
