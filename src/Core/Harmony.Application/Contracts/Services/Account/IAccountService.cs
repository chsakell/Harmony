using Harmony.Application.Requests.Identity;
using Harmony.Shared.Wrapper;

namespace Harmony.Application.Contracts.Services.Account
{
    /// <summary>
    /// Service for updating profile details
    /// </summary>
    public interface IAccountService
    {
        Task<IResult> UpdateProfileAsync(UpdateProfileRequest model, string userId);
        Task<IResult> ChangePasswordAsync(ChangePasswordRequest model, string userId);
        Task<IResult<string>> UpdateProfilePictureAsync(UpdateProfilePictureRequest request, string userId);
    }
}