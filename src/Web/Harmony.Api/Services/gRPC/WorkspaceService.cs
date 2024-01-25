using Grpc.Core;
using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;

namespace Harmony.Api.Services.gRPC
{
    public class WorkspaceService : Protos.WorkspaceService.WorkspaceServiceBase
    {
        private readonly ILogger<WorkspaceService> _logger;
        private readonly IWorkspaceRepository _workspaceRepository;

        public WorkspaceService(ILogger<WorkspaceService> logger, IWorkspaceRepository workspaceRepository)
        {
            _logger = logger;
            _workspaceRepository = workspaceRepository;
        }

        public override async Task<Protos.WorkspaceResponse> GetWorkspace(Protos.WorkspaceFilterRequest request,
            ServerCallContext context)
        {
            var workspace = await _workspaceRepository
                .GetAsync(Guid.Parse(request.WorkspaceId));

            return MapToProto(workspace);
        }


        private Protos.WorkspaceResponse MapToProto(Workspace workspace)
        {
            if (workspace == null)
            {
                return new Protos.WorkspaceResponse()
                {
                    Found = false
                };
            }

            return new Protos.WorkspaceResponse()
            {
                Found = true,
                Workspace = MapToProtoWorkspace(workspace)
            };
        }

        private Protos.Workspace MapToProtoWorkspace(Workspace workspace)
        {
            var proto = new Protos.Workspace()
            {
                Id = workspace.Id.ToString(),
                Name = workspace.Name
            };

            return proto;
        }
    }
}
