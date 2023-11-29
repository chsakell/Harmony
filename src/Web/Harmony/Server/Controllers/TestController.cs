using Harmony.Application.Contracts.Messaging;
using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Commands.UploadCardFile;
using Harmony.Application.Features.Users.Commands.UploadProfilePicture;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Server.Controllers
{
    public class TestController : BaseApiController<TestController>
    {
        private readonly INotificationsPublisher _messageProducer;

        public TestController(INotificationsPublisher messageProducer)
        {
            _messageProducer = messageProducer;
        }

        [HttpPost]
        public IActionResult SendMessage()
        {
            var label = new LabelDto()
            {
                Id = Guid.NewGuid(),
                Colour = "#12345",
                IsChecked = true,
                Title = "hello from rabbit"
            };

            _messageProducer.Publish(label);

            return Ok();
        }
    }
}
