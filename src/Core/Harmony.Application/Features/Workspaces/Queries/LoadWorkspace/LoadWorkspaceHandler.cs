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

namespace Harmony.Application.Features.Workspaces.Queries.LoadWorkspace
{
    public class LoadWorkspaceHandler : IRequestHandler<LoadWorkspaceQuery, IResult<List<LoadWorkspaceResponse>>>
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<LoadWorkspaceHandler> _localizer;
        private readonly IMapper _mapper;

        public LoadWorkspaceHandler(IWorkspaceRepository workspaceRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<LoadWorkspaceHandler> localizer,
            IMapper mapper)
        {
            _workspaceRepository = workspaceRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IResult<List<LoadWorkspaceResponse>>> Handle(LoadWorkspaceQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<List<LoadWorkspaceResponse>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userBoards = await _workspaceRepository.LoadWorkspace(userId, request.WorkspaceId);

            var result = _mapper.Map<List<LoadWorkspaceResponse>>(userBoards);

            return await Result<List<LoadWorkspaceResponse>>.SuccessAsync(result);
        }
    }
}
