using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.SourceControl.DTO;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.SourceControl.Features.SourceControl.Queries.GetCardBranches
{
    internal class GetCardBranchesHandler : IRequestHandler<GetCardBranchesQuery, IResult<List<BranchDto>>>
    {
        private readonly ISourceControlRepository _sourceControlRepository;
        private readonly IMapper _mapper;

        public GetCardBranchesHandler(ISourceControlRepository sourceControlRepository,
            IMapper mapper)
        {
            _sourceControlRepository = sourceControlRepository;
            _mapper = mapper;
        }

        public async Task<IResult<List<BranchDto>>> Handle(GetCardBranchesQuery request, CancellationToken cancellationToken)
        {
            var result = new List<BranchDto>();

            var branches = await _sourceControlRepository.SearchBranches(request.SerialKey);

            if(!branches.Any())
            {
                return Result<List<BranchDto>>.Success(result);
            }

            var repositoryIds = branches.Select(branch => branch.RepositoryId).Distinct().ToList();

            var repositories = await _sourceControlRepository.GetRepositories(repositoryIds);

            foreach(var branch in branches)
            {
                var branchRepository = repositories
                    .FirstOrDefault(repo => repo.RepositoryId == branch.RepositoryId);

                var branchDto = _mapper.Map<BranchDto>(branch);
                branchDto.RepositoryName = branchRepository.Name;
                branchDto.RepositoryUrl = branchRepository.Url;

                result.Add(branchDto);
            }

            return Result<List<BranchDto>>.Success(result);
        }
    }
}
