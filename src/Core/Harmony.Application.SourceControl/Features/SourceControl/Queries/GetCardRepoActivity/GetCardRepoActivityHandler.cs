using Harmony.Application.Contracts.Repositories;
using Harmony.Application.DTO.SourceControl;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.SourceControl.Features.SourceControl.Queries.GetCardRepoActivity
{
    internal class GetCardRepoActivityHandler : IRequestHandler<GetCardRepoActivityQuery, IResult<CardRepoActivityDto>>
    {
        private readonly ISourceControlRepository _sourceControlRepository;

        public GetCardRepoActivityHandler(ISourceControlRepository sourceControlRepository)
        {
            _sourceControlRepository = sourceControlRepository;
        }

        public async Task<IResult<CardRepoActivityDto>> Handle(GetCardRepoActivityQuery request, CancellationToken cancellationToken)
        {
            var totalBranches = await _sourceControlRepository.GetTotalBranches(request.SerialKey);
            var totalPushes = await _sourceControlRepository.GetTotalPushes(request.SerialKey);

            var result = new CardRepoActivityDto()
            {
                Provider = Domain.Enums.SourceControl.SourceControlProvider.GitHub,
                TotalBranches = (int)totalBranches,
                TotalPushed = (int)totalPushes
            };

            return Result<CardRepoActivityDto>.Success(result);
        }
    }
}
