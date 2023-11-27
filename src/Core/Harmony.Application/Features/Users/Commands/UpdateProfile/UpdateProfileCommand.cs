using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Users.Commands.UpdateProfile;

/// <summary>
/// Command for updating profile
/// </summary>
public class UpdateProfileCommand : IRequest<Result<UpdateProfileResponse>>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
}
