namespace Harmony.Application.Events
{
    public class UserProfilePictureUpdated
    {
        public UserProfilePictureUpdated(string profilePicture)
        {
            ProfilePicture = profilePicture;
        }

        public string ProfilePicture { get; set; }
    }
}
