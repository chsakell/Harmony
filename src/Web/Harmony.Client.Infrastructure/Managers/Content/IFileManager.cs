using Harmony.Application.Features.Cards.Commands.UploadFile;
using Harmony.Shared.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Client.Infrastructure.Managers.Content
{
    public interface IFileManager : IManager
    {
        Task<IResult<UploadFileResponse>> UploadFile(UploadFileCommand command);
    }
}
