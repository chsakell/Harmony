using Harmony.Domain.Enums.SourceControl;
using Harmony.Domain.SourceControl;
using Harmony.Shared.Wrapper;
using MediatR;


namespace Harmony.Application.Features.SourceControl.Commands.GetOrCreateRepository
{
    public class GetOrCreateRepositoryCommand : IRequest<Result<Repository>>
    {
        public string RepositoryId { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public SourceControlProvider Provider { get; set; }
    }
}
