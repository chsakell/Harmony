using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Features.Boards.Commands.AddUserBoard;
using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Automations.Commands.CreateAutomation
{
    internal class CreateAutomationCommandHandler
        : IRequestHandler<CreateAutomationCommand, Result<bool>>
    {
        private readonly IAutomationRepository _automationRepository;

        public CreateAutomationCommandHandler(IAutomationRepository automationRepository)
        {
            _automationRepository = automationRepository;
        }

        public async Task<Result<bool>> Handle(CreateAutomationCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
