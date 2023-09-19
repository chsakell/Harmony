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

namespace Harmony.Application.Features.Workspaces.Queries.GetAllForUser
{
    public class GetAllForUserWorkspacesHandler : IRequestHandler<GetAllForUserWorkspacesQuery, IResult<List<GetAllForUserWorkspaceResponse>>>
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<GetAllForUserWorkspacesHandler> _localizer;
        private readonly IMapper _mapper;

        public GetAllForUserWorkspacesHandler(IWorkspaceRepository workspaceRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<GetAllForUserWorkspacesHandler> localizer,
            IMapper mapper)
        {
            _workspaceRepository = workspaceRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IResult<List<GetAllForUserWorkspaceResponse>>> Handle(GetAllForUserWorkspacesQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<List<GetAllForUserWorkspaceResponse>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userWorkspaces = await _workspaceRepository.GetAllForUser(userId);

            var result = _mapper.Map<List<GetAllForUserWorkspaceResponse>>(userWorkspaces);

            return await Result<List<GetAllForUserWorkspaceResponse>>.SuccessAsync(result);
        }
    }
}
