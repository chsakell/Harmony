using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.SourceControl;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.SourceControl.Commands.DeleteBranch
{
    public class DeleteBranchCommandHandler : IRequestHandler<DeleteBranchCommand, Result<bool>>
    {
        private readonly ISourceControlRepository _sourceControlRepository;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IStringLocalizer<DeleteBranchCommandHandler> _localizer;

        public DeleteBranchCommandHandler(ISourceControlRepository sourceControlRepository,
            INotificationsPublisher notificationsPublisher,
            IStringLocalizer<DeleteBranchCommandHandler> localizer)
        {
            _sourceControlRepository = sourceControlRepository;
            _notificationsPublisher = notificationsPublisher;
            _localizer = localizer;
        }

        public async Task<Result<bool>> Handle(DeleteBranchCommand request, CancellationToken cancellationToken)
        {
            Branch branch = await _sourceControlRepository.GetBranch(request.Name, request.RepositoryId);

            if (branch == null)
            {
                return await Result<bool>.FailAsync(_localizer["Branch doesn't exist"]);
            }

            await _sourceControlRepository.DeleteBranch(request.Name);

            return Result<bool>.Success(true, _localizer["Branch deleted"]);
        }
    }
}
