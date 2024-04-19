using AutoMapper;
using Grpc.Core;
using Harmony.Application.DTO.SourceControl;
using Harmony.Application.SourceControl.Features.SourceControl.Queries.GetCardBranches;
using Harmony.Application.SourceControl.Features.SourceControl.Queries.GetCardRepoActivity;
using Harmony.Integrations.SourceControl.Protos;
using MediatR;

namespace Harmony.Integrations.SourceControl.Services
{
    public class SourceControlService : Protos.SourceControlService.SourceControlServiceBase
    {
        private readonly IMediator _mediator;

        public SourceControlService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async override Task<GetCardRepoActivityResponse> GetCardRepoActivity(GetCardRepoActivityRequest request, ServerCallContext context)
        {
            var result = await _mediator.Send(new GetCardRepoActivityQuery(request.SerialKey));

            var response = new GetCardRepoActivityResponse()
            {
                Success = result.Succeeded
            };

            if (result.Succeeded)
            {
                response.Provider = (int)result.Data.Provider;
                response.TotalBranches = result.Data.TotalBranches;
                response.TotalPushed = result.Data.TotalPushed;
            }

            response.Messages.AddRange(result.Messages);

            return response;
        }

        public async override Task<GetCardBranchesResponse> GetCardBranches(GetCardBranchesRequest request, ServerCallContext context)
        {
            var result = await _mediator.Send(new GetCardBranchesQuery(request.SerialKey));

            var response = new GetCardBranchesResponse()
            {
                Success = result.Succeeded
            };

            if (result.Succeeded)
            {
                response.Branches.AddRange(result.Data.Select(MapToProto));
            }

            response.Messages.AddRange(result.Messages);

            return response;
        }

        private CardBranch MapToProto(BranchDto branchDto)
        {
            return new CardBranch()
            {
                Id = branchDto.ToString(),
                Name = branchDto.Name,
                Provider = (int)branchDto.Provider
            };
        }
    }
}
