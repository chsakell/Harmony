using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Users.Queries.GetUserNotifications
{
    /// <summary>
    /// The logged-in user's own notification settings. The handler takes the user from the login, so the query carries
    /// no ID (ADR 2026-11-27-api-authorization-model, rule 4).
    /// </summary>
    public class GetUserNotificationsQuery : IRequest<IResult<List<EmailNotificationType>>>
    {
    }
}