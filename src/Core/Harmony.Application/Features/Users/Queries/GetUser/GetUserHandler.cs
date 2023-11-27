using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Responses;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Users.Queries.GetUser
{
    public class GetUserHandler : IRequestHandler<GetUserQuery, IResult<UserResponse>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<GetUserHandler> _localizer;
        private readonly IUserService _userService;

        public GetUserHandler(ICurrentUserService currentUserService,
            IStringLocalizer<GetUserHandler> localizer,
            IUserService userService)
        {
            _currentUserService = currentUserService;
            _localizer = localizer;
            _userService = userService;
        }

        public async Task<IResult<UserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {

                return await Result<UserResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var user = await _userService.GetAsync(userId);

            return await Result<UserResponse>.SuccessAsync(user.Data);
        }
    }
}
