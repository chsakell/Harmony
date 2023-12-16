using Harmony.Application.Models;
using Harmony.Domain.Enums;

namespace Harmony.Application.Requests
{
    public class UploadRequest : BaseBoardCommand
    {
        public string FileName { get; set; }
        public string Extension { get; set; }
        public AttachmentType Type { get; set; }
        public byte[] Data { get; set; }
    }
}