using Harmony.Application.Events;
using Harmony.Application.Features.Cards.Commands.UploadCardFile;
using Harmony.Application.Features.Users.Commands.UploadProfilePicture;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Content
{
    public class FileManager : IFileManager
    {
        private readonly HttpClient _client;
        public event EventHandler<UserProfilePictureUpdated> OnUserProfilePictureUpdated;

        public FileManager(HttpClient client)
        {
            _client = client;
        }

        public async Task<IResult<UploadCardFileResponse>> UploadFile(UploadCardFileCommand command)
        {
            var response = await _client.PostAsJsonAsync(Routes.FileEndpoints.Index, command);

            return await response.ToResult<UploadCardFileResponse>();
        }

        public async Task<IResult<UploadProfilePictureResponse>> UploadProfilePicture(UploadProfilePictureCommand command)
        {
            var response = await _client.PostAsJsonAsync(Routes.FileEndpoints.ProfilePicture, command);

            var result = await response.ToResult<UploadProfilePictureResponse>();

            if(result.Succeeded)
            {
                OnUserProfilePictureUpdated?
                    .Invoke(this, new UserProfilePictureUpdated(result?.Data?.ProfilePicture));
            }

            return result;
        }
    }
}
