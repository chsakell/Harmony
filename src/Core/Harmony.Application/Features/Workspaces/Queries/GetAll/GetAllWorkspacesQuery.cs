using Harmony.Application.Features.Workspaces.Queries.GetAll;
using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorHero.CleanArchitecture.Application.Features.Documents.Queries.GetAll
{
    public class GetAllWorkspacesQuery : IRequest<IResult<List<GetAllWorkspacesResponse>>>
    {
    }
}