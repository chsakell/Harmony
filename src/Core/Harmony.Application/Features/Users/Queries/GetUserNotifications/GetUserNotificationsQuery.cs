using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Users.Queries.GetUserNotifications
{
    public class GetUserNotificationsQuery : IRequest<IResult<List<EmailNotificationType>>>
    {
        public string UserId { get; set; }

        public GetUserNotificationsQuery(string userId)
        {
            UserId = userId;
        }
    }
}