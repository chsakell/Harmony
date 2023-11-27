namespace Harmony.Application.Features.Users.Commands.UpdateProfile;

/// <summary>
/// Response for updating password
/// </summary>
public class UpdateProfileResponse
{
    public string UserId { get; set; }
    public string ProfilePicture {  get; set; }
}
