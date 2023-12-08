using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    /// <summary>
    /// Repository to access comments
    /// </summary>
    public interface ICommentRepository
    {
        IQueryable<Comment> Entities { get; }
        Task<Comment?> GetComment(Guid commentId);
        Task<List<Comment>> GetComments(Guid cardId);
        Task AddAsync(Comment comment);
        Task<int> CreateAsync(Comment comment);
        Task<int> Update(Comment comment);
        Task<int> Delete(Comment comment);
    }
}
