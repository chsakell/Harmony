using Harmony.Application.Features.Cards.Commands.UploadCardFile;
using Harmony.Application.Features.Users.Commands.UploadProfilePicture;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Api.Controllers.Content
{
    public class FilesController : BaseApiController<FilesController>
    {
        private readonly ISender _sender;

        public FilesController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadCardFile([FromBody] UploadCardFileCommand command)
        {
            return Ok(await _sender.Send(command));
        }

        [HttpPost("profile-picture")]
        [Authorize]
        public async Task<IActionResult> UploadProfilePicture([FromBody] UploadProfilePictureCommand command)
        {
            return Ok(await _sender.Send(command));
        }
    }
}
