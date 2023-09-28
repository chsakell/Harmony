﻿using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;

namespace Harmony.Application.Features.Workspaces.Commands.Create
{
    public class CreateWorkspaceCommandHandler : IRequestHandler<CreateWorkspaceCommand, Result<Guid>>
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IUserWorkspaceRepository _userWorkspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<CreateWorkspaceCommandHandler> _localizer;

        public CreateWorkspaceCommandHandler(IWorkspaceRepository workspaceRepository,
            IUserWorkspaceRepository userWorkspaceRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<CreateWorkspaceCommandHandler> localizer)
        {
            _workspaceRepository = workspaceRepository;
            _userWorkspaceRepository = userWorkspaceRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }
        public async Task<Result<Guid>> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if(string.IsNullOrEmpty(userId))
            {
                return await Result<Guid>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var workspace = new Workspace()
            {
                Name = request.Name,
                Description = request.Description,
                UserId = userId,
                IsPublic = request.IsPublic,
            };

            await _workspaceRepository.AddAsync(workspace);

            var userWorkspace = new UserWorkspace()
            {
                UserId = userId,
                WorkspaceId = workspace.Id
            };

            var dbResult = await _userWorkspaceRepository.CreateAsync(userWorkspace);

            if (dbResult > 0)
            {
                return await Result<Guid>.SuccessAsync(workspace.Id, _localizer["Workspace Created"]);
            }

            return await Result<Guid>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
