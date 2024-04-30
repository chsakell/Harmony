using Harmony.Application.SourceControl.DTO;
using Harmony.Domain.SourceControl;

namespace Harmony.Application.SourceControl.Messages
{
    public class TagPushedMessage
    {
        public string SerialKey { get; set; }
        public string Branch { get; set; }
        public string Tag { get; set; }
    }
}
