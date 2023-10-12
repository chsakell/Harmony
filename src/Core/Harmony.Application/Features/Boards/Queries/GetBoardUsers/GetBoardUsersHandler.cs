﻿using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.DTO;
using Harmony.Application.Responses;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Extensions;

namespace Harmony.Application.Features.Boards.Queries.GetBoardUsers
{
    public class GetBoardUsersHandler : IRequestHandler<GetBoardUsersQuery, Result<List<UserBoardResponse>>>
    {
        private readonly IUserBoardRepository _userBoardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBoardRepository _boardRepository;
        private readonly IUserService _userService;
        private readonly IStringLocalizer<GetBoardUsersHandler> _localizer;
        private readonly IMapper _mapper;

        public GetBoardUsersHandler(IUserBoardRepository userBoardRepository,
            ICurrentUserService currentUserService,
            IBoardRepository boardRepository,
            IUserService userService,
            IStringLocalizer<GetBoardUsersHandler> localizer,
            IMapper mapper)
        {
            _userBoardRepository = userBoardRepository;
            _currentUserService = currentUserService;
            _boardRepository = boardRepository;
            _userService = userService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<Result<List<UserBoardResponse>>> Handle(GetBoardUsersQuery request, CancellationToken cancellationToken)
        {
            var boardMembers = await _userBoardRepository.GetBoardAccessMembers(request.BoardId);

            return await Result<List<UserBoardResponse>>.SuccessAsync(boardMembers);
        }
    }
}
