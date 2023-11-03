using Harmony.Application.DTO;

namespace Harmony.Application.Features.Users.Commands.UploadProfilePicture
{
    public class UploadProfilePictureResponse
    {
        public string UserId { get; set; }
        public string ProfilePicture {  get; set; }
    }
}
