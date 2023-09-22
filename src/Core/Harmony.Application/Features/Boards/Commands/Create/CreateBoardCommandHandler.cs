using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;

namespace Harmony.Application.Features.Boards.Commands.Create
{
    public class CreateBoardCommandHandler : IRequestHandler<CreateBoardCommand, Result<Guid>>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<CreateBoardCommandHandler> _localizer;

        public CreateBoardCommandHandler(IBoardRepository boardRepository,
            ICurrentUserService currentUserService,
            IStringLocalizer<CreateBoardCommandHandler> localizer)
        {
            _boardRepository = boardRepository;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }
        public async Task<Result<Guid>> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<Guid>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var Board = new Board()
            {
                Title = request.Title,
                Description = request.Description,
                WorkspaceId = Guid.Parse(request.WorkspaceId),
                UserId = userId,
                Visibility = request.Visibility
            };

            var dbResult = await _boardRepository.CreateAsync(Board);

            if (dbResult > 0)
            {
                return await Result<Guid>.SuccessAsync(Board.Id, _localizer["Board Created"]);
            }

            return await Result<Guid>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
