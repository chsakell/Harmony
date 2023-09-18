using AutoMapper;
using BlazorHero.CleanArchitecture.Application.Features.Documents.Queries.GetAll;
using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Workspaces.Queries.GetAll
{
    public class GetAllWorkspacesHandler : IRequestHandler<GetAllWorkspacesQuery, IResult<List<GetAllWorkspacesResponse>>>
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IMapper _mapper;

        public GetAllWorkspacesHandler(IWorkspaceRepository workspaceRepository, IMapper mapper)
        {
            _workspaceRepository = workspaceRepository;
            _mapper = mapper;
        }

        public async Task<IResult<List<GetAllWorkspacesResponse>>> Handle(GetAllWorkspacesQuery request, CancellationToken cancellationToken)
        {
            await Task.Delay(500);

            return null;
        }
    }
}
