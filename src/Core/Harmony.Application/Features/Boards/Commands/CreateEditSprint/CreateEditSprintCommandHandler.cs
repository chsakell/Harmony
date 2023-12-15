using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Domain.Enums;
using Harmony.Application.DTO;
using AutoMapper;

namespace Harmony.Application.Features.Boards.Commands.CreateSprint
{
    /// <summary>
    /// Handler for creating sprints
    /// </summary>
    public class CreateEditSprintCommandHandler : IRequestHandler<CreateEditSprintCommand, Result<SprintDto>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ISprintRepository _sprintRepository;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<CreateEditSprintCommandHandler> _localizer;

        public CreateEditSprintCommandHandler(ICurrentUserService currentUserService,
            ISprintRepository sprintRepository,
            IMapper mapper,
            IStringLocalizer<CreateEditSprintCommandHandler> localizer)
        {
            _currentUserService = currentUserService;
            _sprintRepository = sprintRepository;
            _mapper = mapper;
            _localizer = localizer;
        }
        public async Task<Result<SprintDto>> Handle(CreateEditSprintCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<SprintDto>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            if(request.StartDate.HasValue && request.EndDate.HasValue
                && request.StartDate > request.EndDate)
            {
                return await Result<SprintDto>.FailAsync(_localizer["Due date must be greater than start date"]);
            }

            Sprint sprint = null;
            int dbResult;

            if (request.SprintId.HasValue)
            {
                sprint = await _sprintRepository.GetSprint(request.SprintId.Value);

                sprint.Name = request.Name;
                sprint.Goal = request.Goal;
                sprint.StartDate = request.StartDate;
                sprint.EndDate = request.EndDate;

                dbResult = await _sprintRepository.Update(sprint);
            }
            else
            {
                sprint = new Sprint()
                {
                    Name = request.Name,
                    Goal = request.Goal,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    BoardId = request.BoardId,
                    Status = SprintStatus.Idle
                };

                dbResult = await _sprintRepository.CreateAsync(sprint);
            }

            if (dbResult > 0)
            {
                var result = _mapper.Map<SprintDto>(sprint);

                return await Result<SprintDto>.SuccessAsync(result, 
                    request.SprintId.HasValue ? _localizer["Sprint updated"] : _localizer["Sprint created"]);
            }

            return await Result<SprintDto>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
