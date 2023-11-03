using Harmony.Application.Responses;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Users.Queries.GetUser
{
    public class GetUserQuery : IRequest<IResult<UserResponse>>
    {
        public string UserId { get; set; }
    }
}