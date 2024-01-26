using AutoMapper;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Extensions;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Users.Commands.UploadProfilePicture
{
    /// <summary>
    /// Hanlder for uploading profile picture
    /// </summary>
    public class UploadProfilePictureCommandHandler : IRequestHandler<UploadProfilePictureCommand, Result<UploadProfilePictureResponse>>
    {
        private readonly IUploadService _uploadService;
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public UploadProfilePictureCommandHandler(IUploadService uploadService,
            IUserService userService,
            
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _uploadService = uploadService;
            _userService = userService;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<Result<UploadProfilePictureResponse>> Handle(UploadProfilePictureCommand command, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            string profilePictureUrl = null;

            if (command.Data.Length > 0)
            {
                var imageName = _uploadService.UploadAsync(command).Replace(@"\", "/");
                profilePictureUrl = $"files/{command.Type.ToDescriptionString()}/{imageName}";
            }

            var result = await _userService.UpdateUserProfilePicture(userId, profilePictureUrl);

            if (result.Succeeded)
            {
                var response = new UploadProfilePictureResponse()
                {
                    ProfilePicture = profilePictureUrl,
                    UserId = userId
                };

                return await Result<UploadProfilePictureResponse>.SuccessAsync(response, "File uploaded successfully.");
            }

            return Result<UploadProfilePictureResponse>.Fail("Fail to upload image");
        }
    }
}
