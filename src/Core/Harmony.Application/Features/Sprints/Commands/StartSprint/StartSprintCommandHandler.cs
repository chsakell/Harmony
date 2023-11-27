using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Domain.Enums;
using AutoMapper;

namespace Harmony.Application.Features.Sprints.Commands.StartSprint
{
    /// <summary>
    /// Handler for starting sprints
    /// </summary>
    public class StartSprintCommandHandler : IRequestHandler<StartSprintCommand, Result<bool>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ISprintRepository _sprintRepository;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<StartSprintCommandHandler> _localizer;

        public StartSprintCommandHandler(ICurrentUserService currentUserService,
            ISprintRepository sprintRepository,
            IMapper mapper,
            IStringLocalizer<StartSprintCommandHandler> localizer)
        {
            _currentUserService = currentUserService;
            _sprintRepository = sprintRepository;
            _mapper = mapper;
            _localizer = localizer;
        }
        public async Task<Result<bool>> Handle(StartSprintCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var sprint = await _sprintRepository.GetSprint(request.SprintId);

            sprint.Status = SprintStatus.Active;

            var dbResult = await _sprintRepository.Update(sprint);

            if (dbResult > 0)
            {
                return await Result<bool>.SuccessAsync(true, _localizer["Sprint has started"]);
            }

            return await Result<bool>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
