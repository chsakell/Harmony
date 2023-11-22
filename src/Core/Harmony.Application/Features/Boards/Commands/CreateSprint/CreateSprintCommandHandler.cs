using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Shared.Constants.Application;
using Harmony.Domain.Enums;
using Harmony.Application.DTO;
using AutoMapper;

namespace Harmony.Application.Features.Boards.Commands.CreateSprint
{
    /// <summary>
    /// Handler for creating sprints
    /// </summary>
    public class CreateSprintCommandHandler : IRequestHandler<CreateSprintCommand, Result<SprintDto>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ISprintRepository _sprintRepository;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<CreateSprintCommandHandler> _localizer;

        public CreateSprintCommandHandler(ICurrentUserService currentUserService,
            ISprintRepository sprintRepository,
            IMapper mapper,
            IStringLocalizer<CreateSprintCommandHandler> localizer)
        {
            _currentUserService = currentUserService;
            _sprintRepository = sprintRepository;
            _mapper = mapper;
            _localizer = localizer;
        }
        public async Task<Result<SprintDto>> Handle(CreateSprintCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<SprintDto>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var sprint = new Sprint()
            {
                Name = request.Name,
                Goal = request.Goal,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                BoardId = request.BoardId,
                Status = SprintStatus.Idle
            };

            var dbResult = await _sprintRepository.CreateAsync(sprint);

            if (dbResult > 0)
            {
                var result = _mapper.Map<SprintDto>(sprint);

                return await Result<SprintDto>.SuccessAsync(result, _localizer["Sprint Created"]);
            }

            return await Result<SprintDto>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
