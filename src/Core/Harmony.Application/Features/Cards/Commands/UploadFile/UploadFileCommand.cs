using Harmony.Application.Requests;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.UploadFile
{
    public class UploadFileCommand : UploadRequest, IRequest<Result<UploadFileResponse>>
    {
        public Guid CardId { get; set; }
    }
}
