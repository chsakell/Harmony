using Harmony.Application.Requests;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Users.Commands.UploadProfilePicture
{
    public class UploadProfilePictureCommand : UploadRequest, 
            IRequest<Result<UploadProfilePictureResponse>>
    {
    }
}
