using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Features.Cards.Queries.GetCardMembers;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class UserCardRepository : IUserCardRepository
    {
        private readonly HarmonyContext _context;

        public UserCardRepository(HarmonyContext context)
        {
            _context = context;
        }

        public async Task<UserCard?> GetUserCard(Guid cardId, string userId)
        {
            return await _context.UserCards
                .FirstOrDefaultAsync(uc => uc.CardId == cardId && uc.UserId == userId);
        }

        public async Task<List<string>> GetCardMembers(Guid cardId)
        {
            return await _context.UserCards
                .Where(uc => uc.CardId == cardId)
                .Select(uc => uc.UserId)
                .ToListAsync();
        }

        public async Task<int> CreateAsync(UserCard Board)
        {
            await _context.UserCards.AddAsync(Board);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> CountCardUsers(Guid cardId)
        {
            return await _context.UserCards
                .Where(userCard => userCard.CardId == cardId)
                .CountAsync();
        }

        public async Task<List<CardMemberResponse>> GetCardUsers(Guid cardId)
        {
            var boardUsers = await (from userCard in _context.UserCards
                                    join user in _context.Users
                                    on userCard.UserId equals user.Id
                                    where userCard.CardId == cardId
                                    select new CardMemberResponse
                                    {
                                        Id = user.Id,
                                        UserName = user.UserName,
                                        FirstName = user.FirstName,
                                        LastName = user.LastName,
                                        Email = user.Email,
                                        EmailConfirmed = user.EmailConfirmed,
                                        IsActive = user.IsActive,
                                        IsMember = true,
                                        PhoneNumber = user.PhoneNumber
                                    })
                          .ToListAsync();

            return boardUsers;
        }

        public async Task<CardMemberResponse?> GetBoardAccessMember(Guid cardId, string userId)
        {
            var cardUser = await (from userBoard in _context.UserCards
                                    join user in _context.Users
                                    on userBoard.UserId equals user.Id
                                    where userBoard.CardId == cardId && user.Id == userId
                                    select new CardMemberResponse
                                    {
                                        Id = user.Id,
                                        UserName = user.UserName,
                                        FirstName = user.FirstName,
                                        LastName = user.LastName,
                                        Email = user.Email,
                                        EmailConfirmed = user.EmailConfirmed,
                                        IsActive = user.IsActive,
                                        IsMember = true,
                                        PhoneNumber = user.PhoneNumber
                                    })
                          .FirstOrDefaultAsync();

            return cardUser;
        }

        public async Task<int> Delete(UserCard userBoard)
        {
            _context.UserCards.Remove(userBoard);

            return await _context.SaveChangesAsync();
        }
    }
}
