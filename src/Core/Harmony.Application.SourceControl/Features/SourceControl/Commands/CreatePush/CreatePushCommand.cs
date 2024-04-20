using Harmony.Domain.Enums.SourceControl;
using Harmony.Domain.SourceControl;
using Harmony.Shared.Wrapper;
using MediatR;


namespace Harmony.Application.SourceControl.Features.SourceControl.Commands.CreatePush
{
    public class CreatePushCommand : IRequest<Result<bool>>
    {
        public string Branch { get; set; }
        public Repository Repository { get; set; }
        public List<Commit> Commits { get; set; }
    }
}
