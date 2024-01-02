using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Comments.Commands.DeleteComment
{
    public class DeleteCommentCommand : IRequest<Result<bool>>
    {
        public Guid CommentId { get; set; }

        public DeleteCommentCommand(Guid commentId)
        {
            CommentId = commentId;
        }
    }
}
