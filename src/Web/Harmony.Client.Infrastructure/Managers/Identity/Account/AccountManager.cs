using Harmony.Application.Features.Users.Commands.UpdatePassword;
using Harmony.Application.Features.Users.Commands.UpdateProfile;
using Harmony.Application.Requests.Identity;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Client.Infrastructure.Routes;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Identity.Account
{
    public class AccountManager : IAccountManager
    {
        private readonly HttpClient _httpClient;

        public AccountManager(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IResult<UpdatePasswordResponse>> ChangePasswordAsync(UpdatePasswordCommand command)
        {
            var response = await _httpClient.PutAsJsonAsync(AccountEndpoints.ChangePassword, command);
            return await response.ToResult<UpdatePasswordResponse>();
        }

        public async Task<IResult> UpdateProfileAsync(UpdateProfileCommand command)
        {
            var response = await _httpClient.PutAsJsonAsync(AccountEndpoints.UpdateProfile, command);
            return await response.ToResult();
        }

        public async Task<IResult<string>> UpdateProfilePictureAsync(UpdateProfilePictureRequest request, string userId)
        {
            var response = await _httpClient.PostAsJsonAsync(AccountEndpoints.UpdateProfilePicture(userId), request);
            return await response.ToResult<string>();
        }
    }
}