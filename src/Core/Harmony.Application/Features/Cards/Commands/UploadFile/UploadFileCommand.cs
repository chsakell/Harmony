using Harmony.Application.Requests;
using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Cards.Commands.UploadFile
{
    public class UploadFileCommand : UploadRequest, IRequest<Result<UploadFileResponse>>
    {
        public Guid CardId { get; set; }
    }
}
