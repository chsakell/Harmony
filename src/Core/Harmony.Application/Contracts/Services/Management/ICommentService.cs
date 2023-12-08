using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.GetBacklog;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;

namespace Harmony.Application.Contracts.Services.Management
{
    /// <summary>
    /// Service for Comments
    /// </summary>
    public interface ICommentService
	{
        Task<List<CommentDto>> GetCommentsForCard(Guid cardId);
    }
}
