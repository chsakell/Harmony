using Harmony.Domain.Enums.SourceControl;
using Harmony.Domain.SourceControl;
using Harmony.Shared.Wrapper;
using MediatR;


namespace Harmony.Application.SourceControl.Features.SourceControl.Commands.CreatePush
{
    public class CreatePushCommand : IRequest<Result<bool>>
    {
        public string Ref { get; set; }
        public string RepositoryId { get; set; }
        public string PusherName { get; set; }
        public string PusherEmail { get; set; }
        public string SenderLogin { get; set; }
        public string SenderId { get; set; }
        public string SenderAvatarUrl { get; set; }
        public string CompareUrl { get; set; }
        public List<Commit> Commits { get; set; }
    }
}
