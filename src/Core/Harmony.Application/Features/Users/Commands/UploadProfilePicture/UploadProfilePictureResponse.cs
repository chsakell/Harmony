namespace Harmony.Application.Features.Users.Commands.UploadProfilePicture
{
    /// <summary>
    /// Response for uploading profile picture
    /// </summary>
    public class UploadProfilePictureResponse
    {
        public string UserId { get; set; }
        public string ProfilePicture {  get; set; }
    }
}
