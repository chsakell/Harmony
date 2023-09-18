using AutoMapper;
using Harmony.Application.Features.Workspaces.Queries.GetAll;
using Harmony.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Mappings.Management
{
    public class WorkspaceProfile : Profile
    {
        public WorkspaceProfile()
        {
            CreateMap<Workspace, GetUserOwnedWorkspacesResponse>().ReverseMap();
        }
    }
}
