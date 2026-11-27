using Harmony.Application.Features.Users.Commands.UpdatePassword;
using Harmony.Application.Features.Users.Commands.UpdateProfile;
using Harmony.Application.Requests.Identity;
using Harmony.Application.Responses;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Identity.Account
{
    public interface IAccountManager : IManager
    {
        /// <summary>The logged-in user's own account details.</summary>
        Task<IResult<UserResponse>> GetAsync();
        Task<IResult<UpdatePasswordResponse>> ChangePasswordAsync(UpdatePasswordCommand model);
        Task<IResult> UpdateProfileAsync(UpdateProfileCommand command);
        Task<IResult<string>> UpdateProfilePictureAsync(UpdateProfilePictureRequest request, string userId);
    }
}