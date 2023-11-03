using System.ComponentModel;

namespace Harmony.Domain.Enums
{
    public enum AttachmentType : byte
    {
        [Description(@"Cards/Images")]
        CardImage,

        [Description(@"Cards/Documents")]
        CardDocument,

        [Description(@"Users/ProfilePictures")]
        ProfilePicture
    }
}