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

namespace Harmony.Application.Features.Workspaces.Commands.Create
{
    public class CreateWorkspaceCommandHandler : IRequestHandler<CreateWorkspaceCommand, Result<Guid>>
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IStringLocalizer<CreateWorkspaceCommandHandler> _localizer;

        public CreateWorkspaceCommandHandler(IWorkspaceRepository workspaceRepository,
            IStringLocalizer<CreateWorkspaceCommandHandler> localizer)
        {
            _workspaceRepository = workspaceRepository;
            _localizer = localizer;
        }
        public async Task<Result<Guid>> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
        {
            var workspace = new Workspace()
            {
                Name = request.Name,
                Description = request.Description,
            };

            var dbResult = await _workspaceRepository.CreateAsync(workspace);

            if(dbResult > 0)
            {
                return await Result<Guid>.SuccessAsync(workspace.Id, _localizer["Workspace Created"]);
            }

            return await Result<Guid>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
