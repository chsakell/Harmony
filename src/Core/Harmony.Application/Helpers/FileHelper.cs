using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Helpers
{
    /// <summary>
    /// Helper functions for files
    /// </summary>
    public class FileHelper
    {
        public static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPEG", ".JPE", ".BMP", ".GIF", ".PNG" };

        public static AttachmentType GetAttachmentType(string extension)
        {
            return ImageExtensions.Contains(extension.ToUpperInvariant()) ?
                AttachmentType.CardImage : AttachmentType.CardDocument;
        }
    }
}
