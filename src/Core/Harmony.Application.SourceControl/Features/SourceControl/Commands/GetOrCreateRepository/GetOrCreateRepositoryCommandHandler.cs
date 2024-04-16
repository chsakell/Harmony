using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.SourceControl;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.SourceControl.Commands.GetOrCreateRepository
{
    public class GetOrCreateRepositoryCommandHandler : IRequestHandler<GetOrCreateRepositoryCommand, Result<Repository>>
    {
        private readonly ISourceControlRepository _sourceControlRepository;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly IStringLocalizer<GetOrCreateRepositoryCommandHandler> _localizer;

        public GetOrCreateRepositoryCommandHandler(ISourceControlRepository sourceControlRepository,
            INotificationsPublisher notificationsPublisher,
            IStringLocalizer<GetOrCreateRepositoryCommandHandler> localizer)
        {
            _sourceControlRepository = sourceControlRepository;
            _notificationsPublisher = notificationsPublisher;
            _localizer = localizer;
        }

        public async Task<Result<Repository>> Handle(GetOrCreateRepositoryCommand request, CancellationToken cancellationToken)
        {
            var repository = await _sourceControlRepository.GetRepository(request.RepositoryId);

            if(repository == null)
            {
                repository = new Repository()
                {
                    Id = Guid.NewGuid().ToString(),
                    RepositoryId = request.RepositoryId,
                    Url = request.Url,
                    Name = request.Name,
                    FullName = request.FullName,
                    Provider = request.Provider
                };

                await _sourceControlRepository.CreateRepository(repository);
            }

            return Result<Repository>.Success(repository);
        }
    }
}
