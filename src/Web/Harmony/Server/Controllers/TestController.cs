using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications.Email;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Api.Controllers
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
            var notification = new CardDueTimeUpdatedNotification(Guid.Parse("8D7FAE06-D1E3-4418-FD89-08DBF00B9F44"));

            _messageProducer.PublishEmailNotification(notification);

            return Ok();
        }
    }
}
