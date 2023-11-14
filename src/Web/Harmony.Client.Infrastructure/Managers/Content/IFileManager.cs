using Harmony.Application.Events;
using Harmony.Application.Features.Cards.Commands.UploadCardFile;
using Harmony.Application.Features.Users.Commands.UploadProfilePicture;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Content
{
    public interface IFileManager : IManager
    {
        Task<IResult<UploadCardFileResponse>> UploadFile(UploadCardFileCommand command);
        Task<IResult<UploadProfilePictureResponse>> UploadProfilePicture(UploadProfilePictureCommand command);
        event EventHandler<UserProfilePictureUpdated> OnUserProfilePictureUpdated;
    }
}
