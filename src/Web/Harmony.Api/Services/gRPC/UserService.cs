using Grpc.Core;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Responses;

namespace Harmony.Api.Services.gRPC
{
    public class UserService : Protos.UserService.UserServiceBase
    {
        private readonly ILogger<UserCardService> _logger;
        private readonly IUserService _userService;

        public UserService(ILogger<UserCardService> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public override async Task<Protos.UserResponse> GetUser(Protos.UserFilterRequest request,
            ServerCallContext context)
        {
            var userResult = await _userService.GetAsync(request.UserId);

            if (!userResult.Succeeded || userResult.Data == null || !userResult.Data.IsActive)
            {
                return new Protos.UserResponse()
                {
                    Found = false
                };
            }

            var user = userResult.Data;

            return MapToProto(user);
        }

        public async override Task<Protos.UsersResponse> GetUsers(Protos.UsersFilterRequest request, ServerCallContext context)
        {
            var users = (await _userService.GetAllAsync(request.Users));

            var response = new Protos.UsersResponse() { };

            response.Users.AddRange(users.Data.Select(MapToProtoUser));

            return response;
        }

        private Protos.UserResponse MapToProto(UserResponse user)
        {
            return new Protos.UserResponse()
            {
                Found = true,
                User = MapToProtoUser(user)
            };
        }

        private Protos.User MapToProtoUser(UserResponse user)
        {
            return new Protos.User()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName
            };
        }
    }
}
