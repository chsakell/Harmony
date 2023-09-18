using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Features.Workspaces.Queries.GetAll;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Workspaces.Queries.GetUserOwned
{
    public class GetUserOwnedWorkspacesHandler : IRequestHandler<GetUserOwnedWorkspacesQuery, IResult<List<GetUserOwnedWorkspacesResponse>>>
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<GetUserOwnedWorkspacesHandler> _localizer;
        private readonly IMapper _mapper;

        public GetUserOwnedWorkspacesHandler(IWorkspaceRepository workspaceRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<GetUserOwnedWorkspacesHandler> localizer,
            IMapper mapper)
        {
            _workspaceRepository = workspaceRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IResult<List<GetUserOwnedWorkspacesResponse>>> Handle(GetUserOwnedWorkspacesQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<List<GetUserOwnedWorkspacesResponse>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userWorkspaces = await _workspaceRepository.GetUserOwnedWorkspaces(userId);

            var result = _mapper.Map<List<GetUserOwnedWorkspacesResponse>>(userWorkspaces);

            return await Result<List<GetUserOwnedWorkspacesResponse>>.SuccessAsync(result);
        }
    }
}
