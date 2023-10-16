using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.DTO;
using Harmony.Application.Responses;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Extensions;

namespace Harmony.Application.Features.Cards.Queries.GetCardMembers
{
    public class GetCardMembersHandler : IRequestHandler<GetCardMembersQuery, Result<List<CardMemberResponse>>>
    {
        private readonly IUserCardRepository _userCardRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IUserBoardRepository _userBoardRepository;

        public GetCardMembersHandler(IUserCardRepository userCardRepository,
            ICardRepository cardRepository,
            IUserBoardRepository userBoardRepository)
        {
            _userCardRepository = userCardRepository;
            _cardRepository = cardRepository;
            _userBoardRepository = userBoardRepository;
        }

        public async Task<Result<List<CardMemberResponse>>> Handle(GetCardMembersQuery request, CancellationToken cancellationToken)
        {
            var boardId = await _cardRepository.GetBoardId(request.CardId);

            var cardMembers = await _userCardRepository.GetCardUsers(request.CardId);
            var boardMembers = await _userBoardRepository.GetBoardAccessMembers(boardId);

            var members = new List<CardMemberResponse>();

            foreach(var boardMember in boardMembers)
            {
                var isCardMember = cardMembers.Any(mem => mem.Id == boardMember.Id);

                members.Add(new CardMemberResponse()
                {
                    Id = boardMember.Id,
                    Email = boardMember.Email,
                    EmailConfirmed = boardMember.EmailConfirmed,
                    FirstName = boardMember.FirstName,
                    LastName = boardMember.LastName,
                    ProfilePictureDataUrl = boardMember.ProfilePictureDataUrl,
                    UserName = boardMember.UserName,
                    IsMember = isCardMember
                });
            }


            return await Result<List<CardMemberResponse>>.SuccessAsync(members);
        }
    }
}
