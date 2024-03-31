using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Workspaces.Queries.GetWorkspace
{
    public class GetWorkspaceHandler : IRequestHandler<GetWorkspaceQuery, IResult<WorkspaceDto>>
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<GetWorkspaceHandler> _localizer;
        private readonly IMapper _mapper;

        public GetWorkspaceHandler(IWorkspaceRepository workspaceRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<GetWorkspaceHandler> localizer,
            IMapper mapper)
        {
            _workspaceRepository = workspaceRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IResult<WorkspaceDto>> Handle(GetWorkspaceQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<WorkspaceDto>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var workspace = await _workspaceRepository.GetAsync(request.WorkspaceId);

            var result = _mapper.Map<WorkspaceDto>(workspace);

            return await Result<WorkspaceDto>.SuccessAsync(result);
        }
    }
}
