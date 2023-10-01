using Harmony.Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers
{
    public class UserWorkspaceResponse : UserResponse
    {
        public bool IsMember { get; set; }
    }
}
