using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.SourceControl;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.SourceControl.Commands.CreateBranch
{
    public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, Result<bool>>
    {
        private readonly ISourceControlRepository _sourceControlRepository;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IStringLocalizer<CreateBranchCommandHandler> _localizer;

        public CreateBranchCommandHandler(ISourceControlRepository sourceControlRepository,
            INotificationsPublisher notificationsPublisher,
            IStringLocalizer<CreateBranchCommandHandler> localizer)
        {
            _sourceControlRepository = sourceControlRepository;
            _notificationsPublisher = notificationsPublisher;
            _localizer = localizer;
        }

        public async Task<Result<bool>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
        {
            Branch branch = await _sourceControlRepository.GetBranch(request.Name, request.RepositoryId);

            if (branch != null)
            {
                return await Result<bool>.FailAsync(_localizer["Branch already exists"]);
            }

            var repository = await _sourceControlRepository.GetRepository(request.RepositoryId);

            if(repository == null)
            {
                repository = new Repository()
                {
                    Id = Guid.NewGuid().ToString(),
                    RepositoryId = request.RepositoryId,
                    Url = request.RepositoryUrl,
                    Name = request.RepositoryName,
                    FullName = request.RepositoryFullName,
                    Provider = request.Provider
                };

                await _sourceControlRepository.CreateRepository(repository);
            }

            branch = new Branch()
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                RepositoryId = request.RepositoryId
            };

            await _sourceControlRepository.CreateBranch(branch);

            return Result<bool>.Success(true, _localizer["Branch created"]);
        }
    }
}
