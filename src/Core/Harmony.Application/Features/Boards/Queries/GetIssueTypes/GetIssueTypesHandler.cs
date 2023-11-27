using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.DTO;
using Harmony.Application.Features.Workspaces.Queries.GetIssueTypes;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Boards.Queries.GetIssueTypes
{
    public class GetIssueTypesHandler : IRequestHandler<GetIssueTypesQuery, IResult<List<IssueTypeDto>>>
    {
        private readonly IIssueTypeRepository _issueTypeRepository;
        private readonly IStringLocalizer<GetIssueTypesHandler> _localizer;
        private readonly IMapper _mapper;

        public GetIssueTypesHandler(IIssueTypeRepository issueTypeRepository,
            IStringLocalizer<GetIssueTypesHandler> localizer,
            IMapper mapper)
        {

            _issueTypeRepository = issueTypeRepository;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IResult<List<IssueTypeDto>>> Handle(GetIssueTypesQuery request, CancellationToken cancellationToken)
        {
            var issueTypes = await _issueTypeRepository.GetIssueTypes(request.BoardId);

            var result = _mapper.Map<List<IssueTypeDto>>(issueTypes);

            return await Result<List<IssueTypeDto>>.SuccessAsync(result);
        }
    }
}
