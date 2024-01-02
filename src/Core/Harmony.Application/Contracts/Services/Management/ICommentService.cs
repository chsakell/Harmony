using Harmony.Application.DTO;

namespace Harmony.Application.Contracts.Services.Management
{
    /// <summary>
    /// Service for Comments
    /// </summary>
    public interface ICommentService
	{
        Task<List<CommentDto>> GetCommentsForCard(Guid cardId, string userId);
    }
}
