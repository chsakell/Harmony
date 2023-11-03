using Harmony.Application.Requests;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.UploadCardFile
{
    public class UploadCardFileCommand : UploadRequest, IRequest<Result<UploadCardFileResponse>>
    {
        public Guid CardId { get; set; }
    }
}
