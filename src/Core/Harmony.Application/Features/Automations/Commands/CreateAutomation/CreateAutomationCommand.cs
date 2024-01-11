using Harmony.Application.Enums;
using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Automations.Commands.CreateAutomation
{
    public class CreateAutomationCommand : BaseAutomationCommand, IRequest<Result<bool>>
    {

    }
}
