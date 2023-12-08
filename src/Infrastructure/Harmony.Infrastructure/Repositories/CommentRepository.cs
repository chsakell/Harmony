using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly HarmonyContext _context;

        public CommentRepository(HarmonyContext context)
        {
            _context = context;
        }

        public IQueryable<Comment> Entities => _context.Set<Comment>();

        public async Task<Comment?> GetComment(Guid commentId)
        {
            return await _context.Comments
                .FirstOrDefaultAsync(Comment => Comment.Id == commentId);
        }

        public async Task<List<Comment>> GetComments(Guid cardId)
        {
            return await _context.Comments
                .Where(l => l.CardId == cardId)
                .ToListAsync();
        }

        public async Task AddAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
        }

        public async Task<int> CreateAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> Update(Comment Comment)
        {
            _context.Comments.Update(Comment);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(Comment comment)
        {
            _context.Comments.Remove(comment);

            return await _context.SaveChangesAsync();
        }
    }
}
