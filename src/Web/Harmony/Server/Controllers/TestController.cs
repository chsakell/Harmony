using Harmony.Application.Contracts.Messaging;
using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Commands.UploadCardFile;
using Harmony.Application.Features.Users.Commands.UploadProfilePicture;
using Harmony.Application.Notifications;
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
            var notification = new CardDueTimeExpiredNotification()
            {
                Id = Guid.Parse("8D7FAE06-D1E3-4418-FD89-08DBF00B9F44")
            };

            _messageProducer.Publish(notification);

            return Ok();
        }
    }
}
