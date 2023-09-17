using Harmony.Application.Requests;
using Harmony.Application.Requests.Identity;
using Harmony.Shared.Wrapper;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Services.Account
{
    public interface IAccountService
    {
        Task<IResult> UpdateProfileAsync(UpdateProfileRequest model, string userId);

        Task<IResult> ChangePasswordAsync(ChangePasswordRequest model, string userId);

        Task<IResult<string>> GetProfilePictureAsync(string userId);

        Task<IResult<string>> UpdateProfilePictureAsync(UpdateProfilePictureRequest request, string userId);
    }
}