using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Boards.Queries.GetAllForUser
{
    public class GetAllForUserBoardsHandler : IRequestHandler<GetAllForUserBoardsQuery, IResult<List<GetAllForUserBoardResponse>>>
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<GetAllForUserBoardsHandler> _localizer;
        private readonly IMapper _mapper;

        public GetAllForUserBoardsHandler(IWorkspaceRepository workspaceRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<GetAllForUserBoardsHandler> localizer,
            IMapper mapper)
        {
            _workspaceRepository = workspaceRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IResult<List<GetAllForUserBoardResponse>>> Handle(GetAllForUserBoardsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<List<GetAllForUserBoardResponse>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userBoards = await _workspaceRepository.LoadWorkspace(userId, request.WorkspaceId);

            var result = _mapper.Map<List<GetAllForUserBoardResponse>>(userBoards);

            return await Result<List<GetAllForUserBoardResponse>>.SuccessAsync(result);
        }
    }
}
