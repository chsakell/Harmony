
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Cards.Commands.UploadFile
{
    public class UploadFileResponse
    {
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string Url { get; set; }
        public AttachmentType UploadType { get; set; }
    }
}
