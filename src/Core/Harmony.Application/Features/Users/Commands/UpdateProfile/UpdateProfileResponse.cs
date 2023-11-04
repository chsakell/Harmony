using Harmony.Application.DTO;

namespace Harmony.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileResponse
{
    public string UserId { get; set; }
    public string ProfilePicture {  get; set; }
}
