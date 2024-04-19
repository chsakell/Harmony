using Harmony.Application.DTO.SourceControl;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.SourceControl.Features.SourceControl.Queries.GetCardRepoActivity
{
    public class GetCardRepoActivityQuery : IRequest<IResult<CardRepoActivityDto>>
    {
        public GetCardRepoActivityQuery(string serialKey)
        {
            SerialKey = serialKey;
        }

        public string SerialKey { get; set; }
    }
}
