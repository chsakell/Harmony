using Harmony.Application.Responses;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Users.Queries.GetUser
{
    /// <summary>
    /// The logged-in user's own details. The handler takes the user from the login, so the query carries no ID
    /// (ADR 2026-11-27-api-authorization-model, rule 4).
    /// </summary>
    public class GetUserQuery : IRequest<IResult<UserResponse>>
    {
    }
}