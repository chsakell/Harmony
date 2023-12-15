using Harmony.Application.Features.Sprints.Queries.GetSprintReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Services.Management
{
    public interface ISprintService
    {
        Task<GetSprintReportsResponse> GetSprintReports(Guid boardId, Guid sprintId);
    }
}
