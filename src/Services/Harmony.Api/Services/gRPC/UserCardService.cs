using Grpc.Core;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Features.Cards.Commands.AddUserCard;
using Harmony.Domain.Entities;
using MediatR;

namespace Harmony.Api.Services.gRPC
{
    public class UserCardService : Protos.UserCardService.UserCardServiceBase
    {
        private readonly ILogger<UserCardService> _logger;
        private readonly IUserCardRepository _userCardRepository;
        private readonly IMediator _mediator;

        public UserCardService(ILogger<UserCardService> logger, IUserCardRepository userCardRepository,
            IMediator mediator)
        {
            _logger = logger;
            _userCardRepository = userCardRepository;
            _mediator = mediator;
        }

        public override async Task<Protos.UserCardResponse> GetUserCard(Protos.UserCardFilterRequest request,
            ServerCallContext context)
        {
            var userCard = await _userCardRepository
                .GetUserCard(Guid.Parse(request.CardId), request.UserId);

            return MapToProto(userCard);
        }

        public override async Task<Protos.AddUserCardResponse> AddUserCard(Protos.AddUserCardRequest request, ServerCallContext context)
        {
            var result = await _mediator.Send(new AddUserCardCommand(Guid.Parse(request.CardId), request.UserId)
            {
                BoardId = Guid.Parse(request.BoardId)
            });

            return new Protos.AddUserCardResponse()
            {
                Success = result.Succeeded,
                Error = result.Succeeded ? string.Empty : "Failed to assign user to card"
            };
        }

        public override async Task<Protos.IsCardAssignedResponse> IsCardAssigned(Protos.IsCardAssignedRequest request, ServerCallContext context)
        {
            var cardUsers = await _userCardRepository
                .GetCardUsers(Guid.Parse(request.CardId));

            var response = new Protos.IsCardAssignedResponse()
            {
                IsAssigned = cardUsers.Any()
            };

            if(cardUsers.Any())
            {
                response.Users.AddRange(cardUsers.Select(u => u.Id));
            }

            return response;
        }

        private Protos.UserCardResponse MapToProto(UserCard userCard)
        {
            if (userCard == null)
            {
                return new Protos.UserCardResponse()
                {
                    Found = false
                };
            }

            var protoCard = new Protos.UserCard()
            {
                CardId = userCard.CardId.ToString(),
                UserId = userCard.UserId,
            };

            return new Protos.UserCardResponse()
            {
                Found = true,
                UserCard = protoCard
            };
        }
    }
}
