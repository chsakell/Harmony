using Harmony.Application.Features.Cards.Commands.UploadFile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Server.Controllers.Management
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
        public async Task<IActionResult> UploadCardFile([FromBody] UploadFileCommand command)
        {
            return Ok(await _sender.Send(command));
        }
    }
}
