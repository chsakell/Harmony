using Harmony.Application.Features.Cards.Commands.UploadFile;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Content
{
    public interface IFileManager : IManager
    {
        Task<IResult<UploadFileResponse>> UploadFile(UploadFileCommand command);
    }
}
