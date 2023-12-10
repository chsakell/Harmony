using Harmony.Application.DTO;
using Harmony.Application.Features.Comments.Commands.CreateComment;
using Harmony.Application.Features.Comments.Commands.UpdateComment;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface ICommentManager : IManager
    {
		Task<IResult<CreateCommentResponse>> CreateCommentAsync(CreateCommentCommand request);
        Task<IResult<bool>> UpdateCommentAsync(UpdateCommentCommand request);
        Task<IResult<List<CommentDto>>> GetCardComments(Guid cardId);
        Task<IResult<bool>> DeleteComment(Guid commentId);
    }
}
