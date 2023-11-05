using Harmony.Application.Requests;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Users.Commands.UploadProfilePicture
{
    /// <summary>
    /// Command for uploading profile picture
    /// </summary>
    public class UploadProfilePictureCommand : UploadRequest, 
            IRequest<Result<UploadProfilePictureResponse>>
    {
    }
}
